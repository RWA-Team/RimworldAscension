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
            tradeCenter = TradeUtil.FindOccupiedTradeCenter(TradeSession.playerNegotiator);
            switch (ActionToDo)
            {
                case TradeAction.PlayerSells:
                    var colonyPrisoners = thingsColony.Take(Math.Abs(countToDrop)).Cast<Pawn>().ToList();
                    foreach (var pawn in colonyPrisoners)
                    {
                        tradeCenter.colonyExchangeContainer.TryAdd(pawn);
                        tradeCenter.colonyGoodsCost += PriceFor(ActionToDo);
                    }
                    break;
                case TradeAction.PlayerBuys:
                    var traderPrisoners = thingsTrader.Take(Math.Abs(countToDrop)).Cast<Pawn>().ToList();
                    foreach (var pawn in traderPrisoners)
                    {
                        tradeCenter.traderExchangeContainer.TryAdd(pawn);
                        tradeCenter.traderGoodsCost += PriceFor(ActionToDo);
                    }
                    break;
            }
        }
    }
}
