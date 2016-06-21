using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public static class RA_HaulAIUtility
    {
        // added check for number of missing items to haul in container stack
        public static Job HaulMaxNumToCellJob(Pawn hauler, Thing haulable, IntVec3 storeCell, bool fitInStoreCell)
        {
            var job = new Job(JobDefOf.HaulToCell, haulable, storeCell) {maxNumToCarry = 0};

            if (Find.SlotGroupManager.SlotGroupAt(storeCell) != null)
            {
                // container code part
                // list of things on potential cell with container
                var container = Find.ThingGrid.ThingsListAt(storeCell).Find(building => building is Container && building.Spawned) as Container;
                if (container != null)
                {
                    // if container has free slot
                    if (container.StoredItems.Count < container.TryGetComp<CompContainer>().Properties.itemsCap)
                    {
                        // haul maximum possible, excess will be stored in new slot
                        job.maxNumToCarry = haulable.def.stackLimit;
                    }
                    // all slots occupied, find incomplete stacks
                    else
                    {
                        foreach (var item in container.StoredItems.Where(item => item.def == haulable.def && item.stackCount < item.def.stackLimit))
                        {
                            // carry what's left
                            job.maxNumToCarry = item.def.stackLimit - item.stackCount;
                        }
                    }
                }
                // vanilla code part
                else
                {
                    var thing = Find.ThingGrid.ThingAt(storeCell, haulable.def);
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
    }
}
