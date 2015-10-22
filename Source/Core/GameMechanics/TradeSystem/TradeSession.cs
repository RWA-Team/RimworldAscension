using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

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
			TradeSession.deal = new TradeDeal();
		}

        public static void FinishTradeDeal()
        {
            TradeSession.colonyNegotiator = null;
            TradeSession.deal = null;
        }

        public static List<Thing> GenerateTradeCompanyGoods()
        {
            TraderKindDef traderKindDef = new TraderKindDef();
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
