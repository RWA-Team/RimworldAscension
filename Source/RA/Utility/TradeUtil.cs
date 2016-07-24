using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RA
{
    public static class TradeUtil
    {
        public static readonly SimpleCurve TradePricePostFactorCurve = new SimpleCurve
        {
            new CurvePoint(2000f, 1f),
            new CurvePoint(12000f, 0.5f),
            new CurvePoint(200000f, 0.2f)
        };

        public static TradeCenter FindOccupiedTradeCenter(Pawn negotiatior = null)
        {
            return Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>()
                    .FirstOrDefault(tradeCenter => negotiatior == null ? tradeCenter.negotiator != null : tradeCenter.negotiator == negotiatior);
        }

        public static IEnumerable<Thing> AllSellableThings
        {
            get
            {
                if (Current.ProgramState == ProgramState.MapPlaying)
                {
                    var items = new List<Thing>();
                    foreach (var tradeCenter in Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>())
                        items.AddRange(tradeCenter.SellablesAround);
                    return items.Concat(Find.MapPawns.PrisonersOfColonySpawned.Cast<Thing>());
                }
                return null;
            }
        }

        public static float RandomPriceFactorFor(int priceSeed, ThingDef tringDef)
        {
            var thingDefIndex = DefDatabase<ThingDef>.AllDefsListForReading.IndexOf(tringDef);
            Rand.PushSeed();
            Rand.Seed = priceSeed * thingDefIndex;
            var result = Rand.Range(0.9f, 1.1f);
            Rand.PopSeed();
            return result;
        }
    }
}
