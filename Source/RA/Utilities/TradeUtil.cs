using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RA
{
    public static class TradeUtil
    {
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
    }
}
