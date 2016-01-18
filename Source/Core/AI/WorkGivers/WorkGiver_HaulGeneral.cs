using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace RA
{
    public class WorkGiver_HaulGeneral : WorkGiver_Haul
    {        
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (RimWorld.ListerHaulables.ThingsPotentiallyNeedingHauling().Count != ListerHaulables.previousHaulablesCount_Vanilla)
            {
                foreach (Thing thing in RimWorld.ListerHaulables.ThingsPotentiallyNeedingHauling().Except(ListerHaulables.haulables).ToList())
                {
                    ListerHaulables.CheckIfHaulable(thing);
                }

                ListerHaulables.previousHaulablesCount_Vanilla = RimWorld.ListerHaulables.ThingsPotentiallyNeedingHauling().Count;
            }

            ListerHaulables.FindHaulablesInSlotGroups();
            ListerHaulables.FindHaulablesInContainers();

            //Log.Message("vanilla haulables " + RimWorld.ListerHaulables.ThingsPotentiallyNeedingHauling().Count);
            //Log.Message("RA haulables " + ListerHaulables.haulables.Count);
            //Log.Message("containers haulables " + haulables_insideContainers.Count);

            return ListerHaulables.haulables;
        }

        // Skip if no haulables in any list
        public override bool ShouldSkip(Pawn pawn)
        {
            return (ListerHaulables.haulables.Count == 0 && RimWorld.ListerHaulables.ThingsPotentiallyNeedingHauling().Count == 0);
        }

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t))
            {
                return null;
            }
            // call duplicated to make changes
            return HaulToStorageJob(pawn, t);
        }

        // duplicated to make changes
        public Job HaulToStorageJob(Pawn p, Thing t)
        {
            StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(t.Position, t);
            IntVec3 storeCell;
            // call duplicated to make changes
            if (!TryFindBestBetterStoreCellFor(t, p, currentPriority, p.Faction, out storeCell, true))
            {
                JobFailReason.Is("NoEmptyPlace");
                return null;
            }
            // call duplicated to make changes

            return HaulMaxNumToCellJob(p, t, storeCell, false);
        }

        // duplicated to make changes (added container support)
        public Job HaulMaxNumToCellJob(Pawn hauler, Thing haulable, IntVec3 storeCell, bool fitInStoreCell)
        {
            Job job = new Job(JobDefOf.HaulToCell, haulable, storeCell);
            job.maxNumToCarry = 0;

            if (Find.SlotGroupManager.SlotGroupAt(storeCell) != null)
            {
                // container code part
                // list of things on potential cell with container
                List<Thing> list = Find.ThingGrid.ThingsListAt(storeCell);
                if (list.Exists(storage => storage.def.thingClass == typeof(Building_Storage) && storage.TryGetComp<CompContainer>() != null))
                {
                    CompContainer container = list.Find(thing => thing.def.thingClass == typeof(Building_Storage) && thing.TryGetComp<CompContainer>() != null).TryGetComp<CompContainer>();
                    // if container has free slot
                    if (container.itemsCount < container.compProps.itemsCap)
                    {
                        // haul maximum possible, excess will be stored in new slot
                        job.maxNumToCarry = haulable.def.stackLimit;
                    }
                    // all slots occupied, find incomplete stacks
                    else
                    {
                        foreach (Thing item in container.itemsList)
                        {
                            // same thing type in container found
                            if (item.def == haulable.def)
                            {
                                // and it's stack is not full
                                if (item.stackCount < item.def.stackLimit)
                                {
                                    // carry what's left
                                    job.maxNumToCarry = item.def.stackLimit - item.stackCount;
                                }
                            }
                        }
                    }
                }
                // vanilla code part
                else
                {
                    Thing thing = Find.ThingGrid.ThingAt(storeCell, haulable.def);
                    if (thing != null)
                    {
                        job.maxNumToCarry = haulable.def.stackLimit;
                        if (fitInStoreCell)
                        {
                            job.maxNumToCarry -= thing.stackCount;
                        }
                    }
                    else
                    {
                        job.maxNumToCarry = 99999;
                    }
                }
            }
            // no storage on target cell
            else
            {
                job.maxNumToCarry = 99999;
            }
            // check for scattered stacks to pick up too on the move
            job.haulOpportunisticDuplicates = true;
            job.haulMode = HaulMode.ToCellStorage;
            return job;
        }
        
        // duplicated to make changes
        public static bool TryFindBestBetterStoreCellFor(Thing haulable, Pawn carrier, StoragePriority currentPriority, Faction faction, out IntVec3 foundCell, bool needAccurateResult = true)
        {
            List<SlotGroup> allGroupsListInPriorityOrder = Find.SlotGroupManager.AllGroupsListInPriorityOrder;

            if (allGroupsListInPriorityOrder.Count == 0)
            {
                foundCell = IntVec3.Invalid;
                return false;
            }
            ReservationManager reservations = Find.Reservations;
            IntVec3 positionHeld = haulable.PositionHeld;
            StoragePriority storagePriority = currentPriority;
            float num = 2.14748365E+09f;
            IntVec3 intVec = default(IntVec3);
            bool flag = false;
            int count = allGroupsListInPriorityOrder.Count;
            for (int i = 0; i < count; i++)
            {
                SlotGroup slotGroup = allGroupsListInPriorityOrder[i];
                StoragePriority priority = slotGroup.Settings.Priority;
                if (priority < storagePriority || priority <= currentPriority)
                {
                    break;
                }
                if (slotGroup.Settings.AllowedToAccept(haulable))
                {
                    List<IntVec3> cellsList = slotGroup.CellsList;
                    int count2 = cellsList.Count;
                    int num2;
                    if (needAccurateResult)
                    {
                        num2 = Mathf.FloorToInt((float)count2 * Rand.Range(0.005f, 0.018f));
                    }
                    else
                    {
                        num2 = 0;
                    }
                    for (int j = 0; j < count2; j++)
                    {
                        IntVec3 intVec2 = cellsList[j];
                        float lengthHorizontalSquared = (positionHeld - intVec2).LengthHorizontalSquared;
                        if (lengthHorizontalSquared <= num)
                        {
                            if (carrier == null || !intVec2.IsForbidden(carrier))
                            {
                                // call duplicated to make changes
                                if (NoStorageBlockersIn(intVec2, haulable))
                                {
                                    if (!reservations.IsReserved(intVec2, faction))
                                    {
                                        if (!intVec2.ContainsStaticFire())
                                        {
                                            if (carrier == null || haulable.Position.CanReach(slotGroup.CellsList[0], PathEndMode.ClosestTouch, TraverseParms.For(carrier, Danger.Deadly, TraverseMode.ByPawn, false)))
                                            {
                                                flag = true;
                                                intVec = intVec2;
                                                num = lengthHorizontalSquared;
                                                storagePriority = priority;
                                                if (j >= num2)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (!flag)
            {
                foundCell = IntVec3.Invalid;
                return false;
            }
            foundCell = intVec;
            return true;
        }

        // duplicated to make changes (added container support)
        public static bool NoStorageBlockersIn(IntVec3 targetCell, Thing haulable)
        {
            List<Thing> list = Find.ThingGrid.ThingsListAt(targetCell);
            for (int i = 0; i < list.Count; i++)
            {
                Thing currentThing = list[i];

                // container code part
                if (currentThing.def.thingClass == typeof(Building_Storage) && currentThing.TryGetComp<CompContainer>() != null)
                {
                    CompContainer container = list.Find(thing => thing.def.thingClass == typeof(Building_Storage) && thing.TryGetComp<CompContainer>() != null).TryGetComp<CompContainer>();
                    // if has free slots, no blocking
                    if (container.itemsCount < container.compProps.itemsCap)
                    {
                        return true;
                    }
                    // if no slots
                    else
                    {
                        foreach (Thing item in container.itemsList)
                        {
                            // and same thing in container found
                            if (item.def == haulable.def)
                            {
                                // and it's stack is not full
                                if (item.stackCount < item.def.stackLimit)
                                {
                                    // no blocking
                                    return true;
                                }
                            }
                        }
                        // and no matches, block
                        return false;
                    }
                }
                // vanilla code part
                // no incopmlete stacks of the same thing
                if (currentThing.def.EverStoreable)
                {
                    if (currentThing.def != haulable.def)
                    {
                        return false;
                    }
                    if (currentThing.stackCount >= haulable.def.stackLimit)
                    {
                        return false;
                    }
                }
                // placed blueprint not standable
                if (currentThing.def.entityDefToBuild != null && currentThing.def.entityDefToBuild.passability != Traversability.Standable)
                {
                    return false;
                }
                // terrain not standable
                if (currentThing.def.surfaceType == SurfaceType.None && currentThing.def.passability != Traversability.Standable)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
