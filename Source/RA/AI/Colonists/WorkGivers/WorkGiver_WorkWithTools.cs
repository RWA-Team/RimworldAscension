using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using static RA.MapCompDataStorage;

namespace RA
{
    public enum ToolRequirementLevel : byte
    {
        NoToolRequired,
        ToolPreferable,
        ToolRequired
    }

    public abstract class WorkGiver_WorkWithTools : WorkGiver
    {
        public ThingWithComps closestAvailableTool;
        public ToolRequirementLevel targetTRL;

        public virtual string WorkType => default(string);
        public virtual List<TargetInfo> Targets(Pawn pawn) => default(List<TargetInfo>);
        public virtual Job JobWithTool(TargetInfo target) => default(Job);

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // has available job targets
            if (!Targets(pawn).NullOrEmpty())
            {
                var closestAvailableTarget = Targets(pawn).FirstOrDefault().HasThing
                    ? GenClosest.ClosestThing_Global(pawn.Position, Targets(pawn).Select(target => target.Thing))
                    : (TargetInfo) ClosestTargetCell(pawn);

                targetTRL = closestAvailableTarget.HasThing
                    ? (ToolRequirementLevel) closestAvailableTarget.Thing
                        .GetStatValue(StatDef.Named("ToolRequirementLevel"))
                    : (ToolRequirementLevel) closestAvailableTarget.Cell.GetTerrain()
                        .GetStatValueAbstract(StatDef.Named("ToolRequirementLevel"));

                // if proper tool equipped, do the job, otherwise try to find the tool for work, if needed
                return targetTRL == ToolRequirementLevel.NoToolRequired || IsProperTool(pawn.equipment.Primary)
                    ? JobWithTool(closestAvailableTarget)
                    : targetTRL == ToolRequirementLevel.ToolPreferable
                        ? TryEquipTool(pawn) ?? JobWithTool(closestAvailableTarget)
                        : JobWithTool(closestAvailableTarget);
            }

            // drop tool and haul it to stockpile, if necessary
            if (pawn.equipment.Primary != null && !ShouldKeepTool(pawn) &&
                pawn.equipment.Primary.TryGetComp<CompTool>().wasAutoEquipped)
            {
                return TryReturnTool(pawn);
            }
            
            if (PawnCarriedWeaponBefore(pawn))
            {
                EquipPreviousWeapon(pawn);
            }

            return null;
        }

        public IntVec3 ClosestTargetCell(Pawn pawn)
        {
            var searchRange = 9999f;
            var result = default(IntVec3);
            foreach (IntVec3 cell in Targets(pawn))
            {
                var lengthHorizontalSquared = (cell - pawn.Position).LengthHorizontalSquared;
                if (lengthHorizontalSquared < searchRange)
                {
                    result = cell;
                    searchRange = lengthHorizontalSquared;
                }
            }
            return result;
        }

        // keep tool if it could be used for other jobs
        public bool ShouldKeepTool(Pawn pawn)
        {
            if (pawn.equipment.Primary.TryGetComp<CompTool>().Allows("Hunting") &&
                pawn.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && WorkGiver_Hunt.AvailableTargets(pawn).Any())
            {
                return true;
            }
            if (pawn.equipment.Primary.TryGetComp<CompTool>().Allows("Construction") &&
                (pawn.workSettings.WorkIsActive(WorkTypeDefOf.Construction) &&
                 WorkGiver_ConstructFinishFrames.AvailableTargets(pawn).Any()) ||
                (pawn.workSettings.WorkIsActive(WorkTypeDefOf.Repair) && WorkGiver_Repair.AvailableTargets(pawn).Any()))
            {
                return true;
            }
            if (pawn.equipment.Primary.TryGetComp<CompTool>().Allows("Mining") &&
                pawn.workSettings.WorkIsActive(WorkTypeDefOf.Mining) && WorkGiver_Mine.AvailableTargets(pawn).Any())
            {
                return true;
            }
            if (pawn.equipment.Primary.TryGetComp<CompTool>().Allows("Woodchopping") &&
                pawn.workSettings.WorkIsActive(WorkTypeDefOf.PlantCutting) &&
                WorkGiver_ChopWood.AvailableTargets(pawn).Any())
            {
                return true;
            }
            if (pawn.equipment.Primary.TryGetComp<CompTool>().Allows("Digging") &&
                pawn.workSettings.WorkIsActive(WorkTypeDefOf.Growing) &&
                WorkGiver_CultivateLand.AvailableTargets(pawn).Any())
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
            return thing?.TryGetComp<CompTool>()?.Allows(WorkType) ?? false;
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