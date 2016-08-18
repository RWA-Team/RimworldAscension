using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public static class RA_TradeUI
    {
        // RimWorld.TradeUI
        public static void DrawPrice(Rect rect, Tradeable trad, TradeAction action)
        {
            Log.Message("1");
            if (trad.IsCurrency)
            {
                return;
            }
            Log.Message("2");
            if (!trad.TraderWillTrade)
            {
                return;
            }
            Log.Message("3");
            rect = rect.Rounded();
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            Log.Message("4");
            var num = trad.PriceFor(action);
            Log.Message("41");
            var pType = PriceTypeUtlity.ClosestPriceType(num / trad.BaseMarketValue);
            switch (pType)
            {
                case PriceType.VeryCheap:
                    GUI.color = new Color(0f, 1f, 0f);
                    break;
                case PriceType.Cheap:
                    GUI.color = new Color(0.5f, 1f, 0.5f);
                    break;
                case PriceType.Normal:
                    GUI.color = Color.white;
                    break;
                case PriceType.Expensive:
                    GUI.color = new Color(1f, 0.5f, 0.5f);
                    break;
                case PriceType.Exorbitant:
                    GUI.color = new Color(1f, 0f, 0f);
                    break;
            }
            Log.Message("5");
            var label = "$" + num.ToString("F2");
            Func<string> textGetter = delegate
            {
                if (!trad.HasAnyThing)
                {
                    return string.Empty;
                }
                if (!trad.TraderWillTrade)
                {
                    return "TraderWillNotTrade".Translate();
                }
                return ((action != TradeAction.PlayerBuys) ? "SellPriceDesc".Translate() : "BuyPriceDesc".Translate()) + "\n\n" + "PriceTypeDesc".Translate(("PriceType" + pType).Translate());
            };
            Log.Message("6");
            TooltipHandler.TipRegion(rect, new TipSignal(textGetter, trad.GetHashCode() * 297));
            var rect2 = new Rect(rect);
            rect2.xMax -= 5f;
            rect2.xMin += 5f;
            if (Text.Anchor == TextAnchor.MiddleLeft)
            {
                rect2.xMax += 300f;
            }
            if (Text.Anchor == TextAnchor.MiddleRight)
            {
                rect2.xMin -= 300f;
            }
            Log.Message("7");
            Widgets.Label(rect2, label);
            UIUtil.ResetText();
        }

    }
}