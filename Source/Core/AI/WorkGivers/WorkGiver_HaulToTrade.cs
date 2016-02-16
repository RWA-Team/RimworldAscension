using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_HaulToTrade : WorkGiver_Scanner
    {
        public static List<Thing> requestedItems = new List<Thing>();
        public static List<ThingCount> requestedResourceCounters = new List<ThingCount>();

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            IEnumerable<Thing> items = requestedItems;
            foreach (ThingCount counter in requestedResourceCounters)
                items = items.Concat(TradeUtility.AllSellableThings.Where(item => item.def == counter.thingDef));

            return items;
        }

        public override bool ShouldSkip(Pawn pawn)
        {
            return requestedItems.Count == 0 && requestedResourceCounters.Count == 0;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing thing)
        {
            return HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, thing);
        }

        public override Job JobOnThing(Pawn pawn, Thing thing)
        {
            int numToCarry = 1;
            // carry resources according to it's counter number
            if (requestedResourceCounters.Exists(item => item.thingDef == thing.def))
            {
                ThingCount counter = requestedResourceCounters.Find(item => item.thingDef == thing.def);
                numToCarry = Math.Min(thing.stackCount, counter.count);
            }

            return new Job(DefDatabase<JobDef>.GetNamed("HaulToTrade"), thing, FindOccupiedTradingPost())
            {
                maxNumToCarry = numToCarry
            };
        }

        // NOTE: add exception checks
        public static Building_TradingPost FindOccupiedTradingPost()
        {
            return Find.ListerBuildings.AllBuildingsColonistOfClass<Building_TradingPost>().First(post => post.occupied == true);
        }
    }
}
