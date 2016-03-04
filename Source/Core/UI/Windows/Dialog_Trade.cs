using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public class Dialog_Trade : Window
    {
        public const float TitleAreaHeight = 45f;

        public const float TopAreaHeight = 55f;

        public const float ColumnWidth = 120f;

        public const float FirstCommodityY = 6f;

        public const float RowInterval = 30f;

        public Vector2 scrollPosition = Vector2.zero;

        public static float lastCurrencyFlashTime = -100f;

        public List<Tradeable> cachedTradeables;

        public Tradeable cachedCurrencyTradeable;

        public TradeDialogSorter sorter1 = new TradeDialogSorter();

        public TradeDialogSorter sorter2 = new TradeDialogSorter();

        public readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

        public readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

        public override Vector2 InitialWindowSize => new Vector2(1024f, Screen.height);

        public Dialog_Trade()
        {
            ConceptDecider.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
            closeOnEscapeKey = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDef.Named("TradeWindow_Open");
            soundAmbient = SoundDef.Named("TradeWindow_Ambient");
            soundClose = SoundDef.Named("TradeWindow_Close");
            sorter1.name = TradeDialogSorterDefOf.Category.LabelCap;
            sorter1.ComparerSelector(sorter1.name);
            sorter2.name = TradeDialogSorterDefOf.MarketValue.LabelCap;
            sorter2.ComparerSelector(sorter2.name);
        }

        public override void PostOpen()
        {
            base.PostOpen();
            TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);
            if (TradeSession.colonyNegotiator.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking) < 0.99f)
            {
                Find.WindowStack.Add(Dialog_NodeTree.SimpleNotifyDialog("NegotiatorTalkingImpaired".Translate(TradeSession.colonyNegotiator.LabelBaseShort)));
            }
            CacheTradeables();
        }

        public void CacheTradeables()
        {
            cachedCurrencyTradeable = (from x in TradeSession.deal.AllTradeables
                                            where x.IsCurrency
                                            select x).FirstOrDefault();
            cachedTradeables = (from tr in TradeSession.deal.AllTradeables
                                     where tr.TraderWillTrade && !tr.IsCurrency
                                     select tr).OrderByDescending(tr => tr, sorter1.comparer).ThenBy(tr => tr, sorter2.comparer).ThenBy(tr => tr.ListOrderPriority).ThenBy(tr => tr.ThingDef.label).ThenBy(delegate(Tradeable tr)
                                     {
                                         QualityCategory result;
                                         if (tr.AnyThing.TryGetQuality(out result))
                                         {
                                             return (int)result;
                                         }
                                         return -1;
                                     }).ThenBy(tr => tr.AnyThing.HitPoints).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            TradeSession.deal.UpdateCurrencyCount();
            var position = new Rect(0f, 0f, 350f, 27f);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Tiny;
            var rect = new Rect(0f, 0f, 60f, 27f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "SortBy".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            var rect2 = new Rect(rect.xMax + 10f, 0f, 130f, 27f);
            if (Widgets.TextButton(rect2, sorter1.name))
            {
                OpenSorterChangeFloatMenu(0);
            }
            var rect3 = new Rect(rect2.xMax + 10f, 0f, 130f, 27f);
            if (Widgets.TextButton(rect3, sorter2.name))
            {
                OpenSorterChangeFloatMenu(1);
            }
            GUI.EndGroup();
            var num = inRect.width - 590f;
            var position2 = new Rect(num, 0f, inRect.width - num, 55f);
            GUI.BeginGroup(position2);
            Text.Font = GameFont.Medium;
            var rect4 = new Rect(0f, 0f, position2.width / 2f, position2.height);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect4, Find.Map.colonyInfo.ColonyName);
            var rect5 = new Rect(position2.width / 2f, 0f, position2.width / 2f, position2.height);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect5, TradeSession.tradeCompany.name);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            Text.Font = GameFont.Tiny;
            var rect6 = new Rect(position2.width / 2f - 100f - 30f, 0f, 200f, position2.height);
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(rect6, "DragToTrade".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            GUI.EndGroup();
            var num2 = 0f;
            if (cachedCurrencyTradeable != null)
            {
                var num3 = inRect.width - 16f;
                var rect7 = new Rect(0f, 55f, num3, 30f);
                TradeUI.DrawTradeableRow(rect7, cachedCurrencyTradeable, 1);
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(0f, 84f, num3);
                GUI.color = Color.white;
                num2 = 30f;
            }
            var mainRect = new Rect(0f, 55f + num2, inRect.width, inRect.height - 55f - 38f - num2 - 20f);
            FillMainRect(mainRect);
            var rect8 = new Rect(inRect.width / 2f - AcceptButtonSize.x / 2f, inRect.height - 55f, AcceptButtonSize.x, AcceptButtonSize.y);
            if (Widgets.TextButton(rect8, "AcceptButton".Translate()))
            {
                Action action = delegate
                {
                    //bool flag;
                    if (TradeSession.deal.TryMakeDeal())
                    {
                        //if (flag)
                        {
                            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
                            Close(false);
                        }
                        //else
                        {
                            Close();
                        }
                    }
                };
                if (TradeSession.deal.DoesTraderHaveEnoughSilver())
                {
                    action();
                }
                else
                {
                    FlashSilver();
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    Find.WindowStack.Add(new Dialog_Confirm("ConfirmTraderShortFunds".Translate(), action));
                }
                Event.current.Use();
            }
            var rect9 = new Rect(rect8.x - 10f - OtherBottomButtonSize.x, rect8.y, OtherBottomButtonSize.x, OtherBottomButtonSize.y);
            if (Widgets.TextButton(rect9, "ResetButton".Translate()))
            {
                SoundDefOf.TickLow.PlayOneShotOnCamera();
                TradeSession.deal.Reset();
                CacheTradeables();
                Event.current.Use();
            }
            var rect10 = new Rect(rect8.xMax + 10f, rect8.y, OtherBottomButtonSize.x, OtherBottomButtonSize.y);
            if (Widgets.TextButton(rect10, "CancelButton".Translate()))
            {
                Close();
                Event.current.Use();
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            DragSliderManager.ForceStop();
            base.Close(doCloseSound);
        }

        public void FillMainRect(Rect mainRect)
        {
            Text.Font = GameFont.Small;
            var height = 6f + cachedTradeables.Count * 30f;
            var viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            var num = 6f;
            var num2 = scrollPosition.y - 30f;
            var num3 = scrollPosition.y + mainRect.height;
            var num4 = 0;
            foreach (Tradeable tradeable in cachedTradeables)
            {
                if (num > num2 && num < num3)
                {
                    var rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawTradeableRow(rect, tradeable, num4);
                }
                num += 30f;
                num4++;
            }
            Widgets.EndScrollView();
        }
        public static void DrawTradeableRow(Rect rect, Tradeable trad, int index)
        {
            if (index % 2 == 1)
            {
                GUI.DrawTexture(rect, TradeUI.AlternativeBGTex);
            }
            Text.Font = GameFont.Small;
            GUI.BeginGroup(rect);
            var num = rect.width;
            var num2 = trad.CountHeldBy(Transactor.Trader);
            if (num2 != 0)
            {
                var rect2 = new Rect(num - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                }
                Text.Anchor = TextAnchor.MiddleRight;
                var rect3 = rect2;
                rect3.xMin += 5f;
                rect3.xMax -= 5f;
                Widgets.Label(rect3, num2.ToStringCached());
                TooltipHandler.TipRegion(rect2, "TraderCount".Translate());
                var rect4 = new Rect(rect2.x - 100f, 0f, 100f, rect.height);
                Text.Anchor = TextAnchor.MiddleRight;
                TradeUI.DrawPrice(rect4, trad, TradeAction.ToDrop);
            }
            num -= 175f;
            var rect5 = new Rect(num - 240f, 0f, 240f, rect.height);
            TradeUI.DrawCountAdjustInterface(rect5, trad);
            num -= 240f;
            var num3 = trad.CountHeldBy(Transactor.Colony);
            if (num3 != 0)
            {
                var rect6 = new Rect(num - 100f, 0f, 100f, rect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                TradeUI.DrawPrice(rect6, trad, TradeAction.ToLaunch);
                var rect7 = new Rect(rect6.x - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect7))
                {
                    Widgets.DrawHighlight(rect7);
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                var rect8 = rect7;
                rect8.xMin += 5f;
                rect8.xMax -= 5f;
                Widgets.Label(rect8, num3.ToStringCached());
                TooltipHandler.TipRegion(rect7, "ColonyCount".Translate());
            }
            num -= 175f;
            var rect9 = new Rect(0f, 0f, num, rect.height);
            if (Mouse.IsOver(rect9))
            {
                Widgets.DrawHighlight(rect9);
            }
            var rect10 = new Rect(0f, 0f, 27f, 27f);
            Widgets.ThingIcon(rect10, trad.AnyThing);
            Widgets.InfoCardButton(40f, 0f, trad.AnyThing);
            Text.Anchor = TextAnchor.MiddleLeft;
            var rect11 = new Rect(80f, 0f, rect9.width - 80f, rect.height);
            Text.WordWrap = false;
            Widgets.Label(rect11, trad.Label);
            Text.WordWrap = true;
            var localTrad = trad;
            TooltipHandler.TipRegion(rect9, new TipSignal(() => localTrad.Label + ": " + localTrad.TipDescription, localTrad.GetHashCode()));
            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        public void OpenSorterChangeFloatMenu(int sorterIndex)
        {
            var allDefsListForReading = DefDatabase<TradeDialogSorterDef>.AllDefsListForReading;
            var list = allDefsListForReading.Select(def => new FloatMenuOption(def.LabelCap, delegate
            {
                if (sorterIndex == 0)
                {
                    sorter1.name = def.LabelCap;
                    sorter1.ComparerSelector(sorter1.name);
                }
                else
                {
                    sorter2.name = def.LabelCap;
                    sorter2.ComparerSelector(sorter2.name);
                }
                CacheTradeables();
            })).ToList();
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public void FlashSilver()
        {
            lastCurrencyFlashTime = Time.time;
        }
    }
}
