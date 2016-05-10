using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class RA_TradeSession
    {
        public static void SetupWith(ITrader tradeCompany, Pawn negotiator)
        {
            if (!tradeCompany.CanTradeNow)
            {
                Log.Warning("Called SetupWith with a trader not willing to trade now.");
            }
            TradeSession.trader = tradeCompany;
            TradeSession.playerNegotiator = negotiator;

            BindSessionToTradeCenter(tradeCompany, negotiator);

            TradeSession.deal = new TradeDeal();
        }

        public static void BindSessionToTradeCenter(ITrader tradeCompany, Pawn negotiator)
        {
            var tradeCenter = Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>()
                .FirstOrDefault(center => center.trader.TraderKind == tradeCompany.TraderKind && center.trader.TraderName == tradeCompany.TraderName);
            tradeCenter.negotiator = negotiator;
        }
    }
}