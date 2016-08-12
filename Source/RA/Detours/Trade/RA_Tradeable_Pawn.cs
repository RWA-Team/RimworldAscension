using System;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_Tradeable_Pawn : RA_Tradeable
    {
        public override void ResolveTrade()
        {
            if (tradeCenter == null) tradeCenter = TradeUtil.FindOccupiedTradeCenter(TradeSession.playerNegotiator);
            if (ActionToDo == TradeAction.PlayerSells)
            {
                var colonyPrisoners = thingsColony.Take(Math.Abs(countToDrop)).Cast<Pawn>().ToList();
                foreach (var pawn in colonyPrisoners)
                {
                    tradeCenter.colonyExchangeContainer.TryAdd(pawn);
                    tradeCenter.colonyGoodsCost += new Tradeable(pawn, pawn).PriceFor(ActionToDo);
                }
            }
            else if (ActionToDo == TradeAction.PlayerBuys)
            {
                var traderPrisoners = thingsTrader.Take(Math.Abs(countToDrop)).Cast<Pawn>().ToList();
                foreach (var pawn in traderPrisoners)
                {
                    tradeCenter.traderExchangeContainer.TryAdd(pawn);
                    tradeCenter.traderGoodsCost += new Tradeable(pawn, pawn).PriceFor(ActionToDo);
                }
            }
        }
    }
}
