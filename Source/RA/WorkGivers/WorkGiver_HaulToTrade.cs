using System;
using System.Linq;
using RimWorld;
using UnityEngine;
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

                if (closestTradeCenter != null)
                {
                    closestSellable = GenClosest.ClosestThing_Global_Reachable(pawn.Position,
                        closestTradeCenter.pendingItemsCounter, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999,
                        sellable =>
                            pawn.CanReserve(sellable) && !sellable.IsBurning());

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
                numToCarry = Mathf.Min(closestSellable.stackCount,
                    closestTradeCenter.pendingResourcesCounters[closestSellable.def],
                    pawn.carrier.AvailableStackSpace(closestSellable.def));

                closestTradeCenter.pendingResourcesCounters[closestSellable.def] -= numToCarry;
                if (closestTradeCenter.pendingResourcesCounters[closestSellable.def] <= 0)
                {
                    closestTradeCenter.pendingResourcesCounters.Remove(closestSellable.def);
                    // unforbid all remaining pending resources
                    closestTradeCenter.pendingItemsCounter.ForEach(thing => thing.SetForbidden(false));
                    closestTradeCenter.pendingItemsCounter.RemoveAll(resource => resource.def == closestSellable.def);
                } 
            }
            
            // if taking whole remaining stack or required partial bit of it, consider item being hauled and remove it from pending list
            if (numToCarry == closestSellable.stackCount || !closestTradeCenter.pendingResourcesCounters.ContainsKey(closestSellable.def))
            {
                closestTradeCenter.pendingItemsCounter.Remove(closestSellable);
            }
            
            return new Job(DefDatabase<JobDef>.GetNamed("HaulToTrade"), closestSellable, closestTradeCenter)
            {
                maxNumToCarry = numToCarry
            };
        }
    }
}
