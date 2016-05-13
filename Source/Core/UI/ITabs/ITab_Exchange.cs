using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class ITab_Exchange : ITab
    {
        public Vector2 scrollPosition_Colony = Vector2.zero;
        public Vector2 scrollPosition_Trader = Vector2.zero;

        public TradeCenter tradeCenter;

        public ITab_Exchange()
        {
            size = new Vector2(UIUtil.ITabWindowWidth, 400f);
            labelKey = "Exchange";
        }

        // called before OnOpen()
        public override bool IsVisible
        {
            get
            {
                tradeCenter = SelThing as TradeCenter;
                return tradeCenter.colonyExchangeContainer.Any() || tradeCenter.traderExchangeContainer.Any();
            }
        }

        protected override void FillTab()
        {
            var mainRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(UIUtil.DefaultMargin);

            // trade balance label
            var tradeBalanceRect = new Rect(mainRect.x, mainRect.y, 200f, 30f)
                .CenteredOnXIn(mainRect);
            DrawTradeBalance(tradeBalanceRect);

            // column headers labels
            Text.Anchor = TextAnchor.MiddleCenter;
            var colonyLabelRect = new Rect(mainRect.x, tradeBalanceRect.yMax, mainRect.width/2, tradeBalanceRect.height);
            Widgets.Label(colonyLabelRect, "Colony offer:");
            var traderLabelRect = new Rect(colonyLabelRect.xMax, colonyLabelRect.y, colonyLabelRect.width,
                colonyLabelRect.height);
            Widgets.Label(traderLabelRect, "Trader offer:");

            // negate deal button
            var negateButtonRect = new Rect(colonyLabelRect.x, colonyLabelRect.y, 80f, colonyLabelRect.height).CenteredOnXIn(mainRect);
            if (Widgets.TextButton(negateButtonRect, "Negate"))
            {
                tradeCenter.NegateTradeDeal();
                var currentITab = (MainTabWindow_Inspect)MainTabDefOf.Inspect.Window;
                currentITab.CloseOpenTab();
            }

            // exchange table
            var colonyRect = new Rect(mainRect.x, colonyLabelRect.yMax + UIUtil.DefaultMargin, mainRect.width/2,
                mainRect.height - colonyLabelRect.yMax);
            Widgets.DrawWindowBackground(colonyRect);
            UIUtil.DrawItemsList(colonyRect, ref scrollPosition_Colony, tradeCenter.colonyExchangeContainer.ToList(), thing =>
            {
                Thing unused;
                tradeCenter.colonyExchangeContainer.TryDrop(thing, tradeCenter.InteractionCell,
                    ThingPlaceMode.Near, out unused);
                tradeCenter.colonyGoodsCost -= tradeCenter.ThingFinalCost(thing, TradeAction.PlayerSells)*
                                               thing.stackCount;
            });
            var traderRect = new Rect(colonyRect)
            {
                x = colonyRect.xMax
            };
            Widgets.DrawWindowBackground(traderRect);
            UIUtil.DrawItemsList(traderRect, ref scrollPosition_Trader, tradeCenter.traderExchangeContainer.ToList(), thing =>
            {
                tradeCenter.traderExchangeContainer.TransferToContainer(thing, tradeCenter.traderStock, thing.stackCount);
                tradeCenter.traderGoodsCost -= tradeCenter.ThingFinalCost(thing, TradeAction.PlayerBuys)*
                                               thing.stackCount;
            });

            UIUtil.ResetText();
        }

        public void DrawTradeBalance(Rect rect)
        {
            var tradeBalance = tradeCenter.TradeBalance;
            GUI.color = tradeBalance > 0
                ? Color.green
                : tradeBalance < 0
                    ? Color.yellow
                    : Color.white;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, string.Format("<b>Trade balance: {0}</b>", tradeBalance.ToStringMoney()));
            UIUtil.ResetText();
        }
    }
}