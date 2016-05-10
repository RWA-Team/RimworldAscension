using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_HaulToTrade : WorkGiver
    {
        public List<TradeCenter> tradeCenters = new List<TradeCenter>();

        public override bool ShouldSkip(Pawn pawn)
        {
            tradeCenters = Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>()
                .Where(tradeCenter => tradeCenter.pendingItemsCounter.Any()).ToList();

            return !tradeCenters.Any();
        }

        public override Job NonScanJob(Pawn pawn)
        {
            var closestTradeCenter = GenClosest.ClosestThing_Global_Reachable(pawn.Position,
                (IEnumerable<Thing>) tradeCenters, PathEndMode.Touch, TraverseParms.For(pawn)) as TradeCenter;
            var closestSellable = GenClosest.ClosestThing_Global_Reachable(pawn.Position,
                closestTradeCenter.SellablesAround, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999,
                sellable => pawn.CanReserve(sellable) && !sellable.IsBurning());

            if (closestSellable != null)
            {
                var numToCarry = 1;
                if (closestSellable.stackCount > 1)
                {
                    var remainingResourceCount = closestTradeCenter.pendingResourcesCounters[closestSellable.def];
                    numToCarry = Math.Min(closestSellable.stackCount,
                        remainingResourceCount);

                    remainingResourceCount -= numToCarry;
                    if (remainingResourceCount == 0)
                        closestTradeCenter.pendingResourcesCounters.Remove(closestSellable.def);
                }

                return new Job(DefDatabase<JobDef>.GetNamed("HaulToTrade"), closestSellable, closestTradeCenter)
                {
                    maxNumToCarry = numToCarry
                };
            }
            return null;
        }
    }
}
