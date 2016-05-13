using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_HaulToTrade : WorkGiver
    {
        public TradeCenter closestTradeCenter = new TradeCenter();
        public Thing closestSellable = new Thing();

        public override bool ShouldSkip(Pawn pawn)
        {
            var pendingTradeCenters = Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>()
                .Where(tradeCenter => tradeCenter.pendingItemsCounter.Any());

            if (pendingTradeCenters.Any())
            {
                closestTradeCenter = GenClosest.ClosestThing_Global_Reachable(pawn.Position,
                    pendingTradeCenters.Cast<Thing>(), PathEndMode.Touch, TraverseParms.For(pawn)) as TradeCenter;

                if (closestTradeCenter!=null)
                {
                    closestSellable = GenClosest.ClosestThing_Global_Reachable(pawn.Position,
                        closestTradeCenter.pendingItemsCounter, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999,
                        sellable => pawn.CanReserve(sellable) && !sellable.IsBurning());

                    if (closestSellable != null)
                        return false;
                }
            }
            return true;
        }

        public override Job NonScanJob(Pawn pawn)
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
    }
}
