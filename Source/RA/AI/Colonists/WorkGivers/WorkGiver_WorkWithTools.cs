using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using static RA.MapCompDataStorage;

namespace RA
{
    public class WorkGiver_WorkWithTools : WorkGiver
    {
        public ThingWithComps closestAvailableTool;

        public string workType;

        public Job DoJobWithTool(Pawn pawn, IEnumerable<Thing> availableTargets, Func<Thing, Job> ActualJob,
            Func<Pawn, bool> ShouldKeepTool)
        {
            // has available job targets
            if (availableTargets.Any())
            {
                var closestAvailableTarget = GenClosest.ClosestThing_Global(pawn.Position, availableTargets);

                // if proper tool equipped, do the job
                if (IsProperTool(pawn.equipment.Primary))
                {
                    // do the vanilla job with tool in hands
                    return ActualJob(closestAvailableTarget);
                }
                
                // or find the tool for work
                return TryEquipTool(pawn);
            }
            
            // drop tool and haul it to stockpile, if necessary
            if (IsProperTool(pawn.equipment.Primary) && !ShouldKeepTool(pawn) && pawn.equipment.Primary.TryGetComp<CompTool>().wasAutoEquipped)
            {
                return TryReturnTool(pawn);
            }

            if (PawnCarriedWeaponBefore(pawn))
            {
                EquipPreviousWeapon(pawn);
            }

            return null;
        }

        // keep tool if it could be used for other jobs
        public virtual bool ShouldKeepTool(Pawn pawn)
        {
            // pawn is ready to hunt
            if (pawn.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) &&
                (pawn.equipment.Primary?.TryGetComp<CompTool>()?.Allows("Hunting") ?? false) &&
                WorkGiver_Hunt.AvailableTargets(pawn).Any())
            {
                return true;
            }
            return false;
        }

        public Job TryEquipTool(Pawn pawn)
        {
            return !TryEquipToolFromInventory(pawn) ? TryEquipFreeTool(pawn) : null;
        }

        public bool TryEquipToolFromInventory(Pawn pawn)
        {
            foreach (var thing in pawn.inventory.container)
            {
                if (IsProperTool(thing))
                {
                    previousPawnWeapons.Add(pawn.equipment.Primary, pawn);
                    SwapOrEquipPreviousWeapon(thing, pawn);
                    return true;
                }
            }
            return false;
        }

        public void EquipPreviousWeapon(Pawn pawn)
        {
            SwapOrEquipPreviousWeapon(previousPawnWeapons[pawn], pawn);
            previousPawnWeapons.Remove(pawn);
        }

        // swap weapon in inventory with equipped one, or just equips it
        public void SwapOrEquipPreviousWeapon(Thing thing, Pawn pawn)
        {
            // if pawn has equipped weapon, put it in inventory
            if (pawn.equipment.Primary != null)
            {
                ThingWithComps leftover;
                pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.inventory.container,
                    out leftover);
            }
            // equip new required weapon
            pawn.equipment.AddEquipment(thing as ThingWithComps);
            // remove that equipment from inventory
            pawn.inventory.container.Remove(thing);
        }

        public bool IsProperTool(Thing thing)
        {
            return thing?.TryGetComp<CompTool>()?.Allows(workType) ?? false;
        }

        public Job TryEquipFreeTool(Pawn pawn)
        {
            // find proper tools of the specific work type
            IEnumerable<Thing> availableTools =
                Find.ListerThings.AllThings.FindAll(
                    tool =>
                        IsProperTool(tool) && !tool.IsForbidden(pawn.Faction) &&
                        pawn.CanReserveAndReach(tool, PathEndMode.ClosestTouch, pawn.NormalMaxDanger()));

            if (availableTools.Any())
            {
                // find closest reachable tool of the specific work type
                closestAvailableTool = GenClosest.ClosestThing_Global(pawn.Position, availableTools) as ThingWithComps;

                if (closestAvailableTool != null)
                {
                    // if pawn has equipped weapon, put it in inventory
                    if (pawn.equipment.Primary != null)
                    {
                        previousPawnWeapons.Add(pawn.equipment.Primary, pawn);
                        ThingWithComps leftover;
                        pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.inventory.container,
                            out leftover);
                    }

                    // reserve and set as auto equipped
                    pawn.Reserve(closestAvailableTool);
                    closestAvailableTool.TryGetComp<CompTool>().wasAutoEquipped = true;

                    return new Job(JobDefOf.Equip, closestAvailableTool);
                }
            }

            return null;
        }

        public bool PawnCarriedWeaponBefore(Pawn pawn) => pawn.equipment.Primary != null && previousPawnWeapons.ContainsKey(pawn.equipment.Primary) &&
                                                          previousPawnWeapons[pawn.equipment.Primary] == pawn;

        // drop tool and haul it to stockpile, if necessary
        public Job TryReturnTool(Pawn pawn)
        {
            ThingWithComps tool;
            // drops primary equipment (weapon) as forbidden
            pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out tool, pawn.Position);
            // making it non forbidden
            tool.SetForbidden(false);

            // tr to haul to the stockpile
            return HaulAIUtility.HaulToStorageJob(pawn, tool);
        }
    }
}