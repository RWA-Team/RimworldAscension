using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_WorkWithTools : WorkGiver, IExposable
    {
        // reserved weapon reference to swap with tool later
        public ThingWithComps reservedSlotter;
        public ThingWithComps reservedWeapon;

        public Thing closestAvailableTool;
        public string workTypeName;

        public Job DoJobWithTool(Pawn pawn, IEnumerable<Thing> availableTargets, Func<Thing, Job> ActualJob)
        {
            // has available job targets
            if (availableTargets.Count() > 0)
            {
                Thing closestAvailableTarget = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableTargets, PathEndMode.Touch, TraverseParms.For(pawn));

                // hands free
                if (pawn.equipment.Primary == null)
                {
                    // search in slots for proper tool
                    if (!TryEquipToolFromSlots(pawn))
                    {
                        // then search for free tool
                        if (TryFindAvailableTool(pawn))
                        {
                            //reserve target for future work
                            if (pawn.CanReserve(closestAvailableTarget))
                            {
                                pawn.Reserve(closestAvailableTarget);
                            }

                            // equip nearest tool
                            return new Job(JobDefOf.Equip, closestAvailableTool);
                        }
                    }
                }
                // hands occupied
                else
                {
                    // proper tool in hands
                    if (IsProperTool(pawn.equipment.Primary))
                    {
                        // do the vanilla job with tool in hands
                        return ActualJob(closestAvailableTarget);
                    }
                    /*
                    // tool in hands, but not appropriate
                    else if (pawn.equipment.Primary.def.defName.Contains("Tool"))
                    {
                        //reserve plant for future work
                        ReservationUtility.Reserve(pawn, closestAvailablePlant);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                     */
                    // not tool in hands
                    else
                    {
                        // search in slots for proper tool
                        if (!TryEquipToolFromSlots(pawn))
                        {
                            if (!TryStoreCurrentWeaponInSlots(pawn))
                            {
                                // add to reserve list, cause it will be dropped to equip tool later
                                reservedWeapon = pawn.equipment.Primary;
                            }

                            // then search for free tool
                            if (TryFindAvailableTool(pawn))
                            {
                                // equip nearest tool
                                return new Job(JobDefOf.Equip, closestAvailableTool);
                            }
                        }
                    }
                }
            }
            // no available targets
            else
            {
                // hands free
                if (pawn.equipment.Primary == null)
                {
                    // has reserved weapon
                    if (reservedWeapon != null)
                    {
                        // try get it from slots
                        if (!TryEquipReservedWeaponFromSlots(pawn))
                        {
                            // else try pick it up
                            reservedWeapon = null;
                            if (pawn.CanReach(reservedWeapon, PathEndMode.ClosestTouch, pawn.NormalMaxDanger()))
                            {
                                return new Job(JobDefOf.Equip, reservedWeapon);
                            }
                        }
                    }
                }
                // hands occupied
                else
                {
                    // proper tool in hands
                    if (IsProperTool(pawn.equipment.Primary))
                    {
                        // keep tool in hands, cause other jobs of the same worktype is being done
                        if (!ShouldKeepTool(pawn))
                        {
                            // has place to store tool
                            if (StorageCellExists(pawn))
                            {
                                // store it
                                return ReturnTool(pawn);
                            }
                            // no storage, but has reserved weapon
                            if (reservedWeapon != null)
                            {
                                // try get it from slots
                                if (!TryEquipReservedWeaponFromSlots(pawn))
                                {
                                    // else try pick it up
                                    reservedWeapon = null;
                                    if (pawn.CanReach(reservedWeapon, PathEndMode.ClosestTouch, pawn.NormalMaxDanger()))
                                    {
                                        return new Job(JobDefOf.Equip, reservedWeapon);
                                    }
                                }
                            }
                            // no storage, no reserve, drop the tool
                            else
                            {
                                ThingWithComps tool;
                                // drops primary equipment (weapon) as forbidden
                                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out tool, pawn.Position);
                                // making it non forbidden
                                tool.SetForbidden(false);
                            }
                        }
                    }
                    // something else in hands, skip job
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public bool IsProperTool(Thing thing)
        {
            return (thing.def.weaponTags != null && thing.def.weaponTags.Exists(tag => tag.Contains("Tool") && tag.Contains(workTypeName))) ? true : false;
        }

        // keeps current tool equipped if there are available unfinished jobs for this tool type
        public bool ShouldKeepTool(Pawn pawn)
        {
            foreach (string tag in pawn.equipment.Primary.def.weaponTags)
            {
                if (tag.Contains("Construction") && (pawn.workSettings.WorkIsActive(WorkTypeDefOf.Construction) && WorkGiver_ConstructFinishFrames.hasPotentialJobs || pawn.workSettings.WorkIsActive(WorkTypeDefOf.Repair) && WorkGiver_Repair.hasPotentialJobs))
                {
                    return true;
                }
                if (pawn.workSettings.WorkIsActive(WorkTypeDefOf.Mining) && tag.Contains("Mining") && WorkGiver_Mine.hasPotentialJobs)
                {
                    return true;
                }
                if (pawn.workSettings.WorkIsActive(WorkTypeDefOf.PlantCutting) && tag.Contains("PlantCutting") && WorkGiver_PlantsCut.hasPotentialJobs)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryEquipToolFromSlots(Pawn pawn)
        {
            IEnumerable<CompSlots> compsSlots = pawn.GetComp<CompEquipmentGizmoUser>().EquippedSlottersComps();

            foreach (CompSlots comp in compsSlots)
            {
                foreach (Thing thing in comp.slots)
                {
                    if (IsProperTool(thing))
                    {
                        if (pawn.equipment.Primary != null)
                        {
                            reservedSlotter = comp.parent;
                            reservedWeapon = pawn.equipment.Primary;
                        }
                        comp.SwapEquipment(thing as ThingWithComps);

                        return true;
                    }
                }
            }

            return false;
        }
        public bool TryEquipReservedWeaponFromSlots(Pawn pawn)
        {
            if (pawn.equipment.AllEquipment.Contains(reservedSlotter) || pawn.apparel.WornApparel.Contains(reservedSlotter as Apparel))
            {
                CompSlots comp = reservedSlotter.GetComp<CompSlots>();

                if (comp.slots.Contains(reservedWeapon))
                {
                    comp.SwapEquipment(reservedWeapon);

                    reservedSlotter = null;
                    reservedWeapon = null;

                    return true;
                }
            }
            reservedSlotter = null;
            reservedWeapon = null;

            return false;
        }

        public bool TryStoreCurrentWeaponInSlots(Pawn pawn)
        {
            IEnumerable<CompSlots> compsSlots = pawn.GetComp<CompEquipmentGizmoUser>().EquippedSlottersComps();

            foreach (CompSlots comp in compsSlots)
            {
                if (comp.slots.Count < comp.Properties.maxSlots)
                {
                    // NOTE: check if weapon reference is destroyed
                    reservedSlotter = comp.parent;
                    reservedWeapon = pawn.equipment.Primary;
                    ThingWithComps resultThing;
                    // put weapon in slotter
                    pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, comp.slots, out resultThing);

                    return true;
                }
            }

            return false;
        }

        public bool TryFindAvailableTool(Pawn pawn)
        {
            // find proper tools of the specific work type
            IEnumerable<Thing> availableTools = Find.ListerThings.AllThings.FindAll(tool => IsProperTool(tool) && !tool.IsForbidden(pawn.Faction) && pawn.CanReserveAndReach(tool, PathEndMode.ClosestTouch, pawn.NormalMaxDanger()));

            if (availableTools.Count() > 0)
            {
                // find closest reachable tool of the specific work type
                closestAvailableTool = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableTools, PathEndMode.ClosestTouch, TraverseParms.For(pawn));

                return true;
            }

            return false;
        }

        public Job ReturnTool(Pawn pawn)
        {
            // NOTE: temporary solution using default jobs, need future improvement
            ThingWithComps tool;
            // drops primary equipment (weapon) as forbidden
            pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out tool, pawn.Position);
            // making it non forbidden
            tool.SetForbidden(false);
            // vanilla haul to best stockpile job
            return HaulAIUtility.HaulToStorageJob(pawn, tool);
        }

        // check if there is free cell is the proper stockpile
        public bool StorageCellExists(Pawn pawn)
        {
            foreach (var slotGroup in Find.SlotGroupManager.AllGroupsListInPriorityOrder)
            {
                foreach (var cell in slotGroup.CellsList)
                {
                    if (StoreUtility.IsValidStorageFor(cell, pawn.equipment.Primary) && pawn.CanReserveAndReach(cell, PathEndMode.OnCell, pawn.NormalMaxDanger()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ExposeData()
        {
            Scribe_References.LookReference<ThingWithComps>(ref reservedSlotter, "reservedSlotter");
            Scribe_References.LookReference<ThingWithComps>(ref reservedWeapon, "reservedWeapon");
        }
    }
}
