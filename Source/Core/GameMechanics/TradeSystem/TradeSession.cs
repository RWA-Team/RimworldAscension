using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class TradeSession
	{
        public static TradeCompany tradeCompany;
		public static Pawn colonyNegotiator;
		public static TradeDeal deal;

        public static void InitiateTradeDeal(Pawn colonyNegotiator)
		{
            TradeSession.colonyNegotiator = colonyNegotiator;
			deal = new TradeDeal();
		}

        public static void FinishTradeDeal()
        {
            colonyNegotiator = null;
            deal = null;
        }

        public static List<Thing> GenerateTradeCompanyGoods()
        {
            var traderKindDef = new TraderKindDef();
            switch (Faction.OfColony.def.techLevel)
            {
                case TechLevel.Neolithic:
                    traderKindDef = DefDatabase<TraderKindDef>.GetNamed("NeolithicTrader");
                    break;
                case TechLevel.Medieval:
                    traderKindDef = DefDatabase<TraderKindDef>.GetNamed("MedievalTrader");
                    break;
            }
            tradeCompany = new TradeCompany(traderKindDef);

            return TraderStockGenerator.GenerateTraderThings(traderKindDef).ToList();
        }

	}
}
