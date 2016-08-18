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
            Log.Message("1");
            if (tradeCenter == null) tradeCenter = TradeUtil.FindOccupiedTradeCenter(TradeSession.playerNegotiator);
            switch (ActionToDo)
            {
                case TradeAction.PlayerSells:
                    var colonyPrisoners = thingsColony.Take(Math.Abs(countToDrop)).Cast<Pawn>().ToList();
                    foreach (var pawn in colonyPrisoners)
                    {
                        Log.Message("2");
                        tradeCenter.colonyExchangeContainer.TryAdd(pawn);
                        tradeCenter.colonyGoodsCost += new Tradeable(pawn, pawn).PriceFor(ActionToDo);
                    }
                    break;
                case TradeAction.PlayerBuys:
                    var traderPrisoners = thingsTrader.Take(Math.Abs(countToDrop)).Cast<Pawn>().ToList();
                    foreach (var pawn in traderPrisoners)
                    {
                        Log.Message("3");
                        tradeCenter.traderExchangeContainer.TryAdd(pawn);
                        tradeCenter.traderGoodsCost += new Tradeable(pawn, pawn).PriceFor(ActionToDo);
                    }
                    break;
            }
        }
    }
}
