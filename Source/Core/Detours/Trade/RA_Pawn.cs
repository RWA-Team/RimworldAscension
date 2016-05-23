using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RA
{
    public class RA_Pawn : Pawn
    {
        public new IEnumerable<Thing> ColonyThingsWillingToBuy => TradeUtil.AllSellableThings;

        // trader.Goods still holds list of caravan chattel pawns, when all items been already transfered to the trade center
        public new IEnumerable<Thing> Goods => Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>()
            .FirstOrDefault(tradeCenter => tradeCenter.trader == this)
            .traderStock.Concat(trader.Goods);
    }
}