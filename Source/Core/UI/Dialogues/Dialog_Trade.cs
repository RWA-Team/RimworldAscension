using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
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

        public override Vector2 InitialWindowSize
        {
            get
            {
                return new Vector2(1024f, (float)Screen.height);
            }
        }

        public Dialog_Trade()
        {
            ConceptDecider.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
            this.closeOnEscapeKey = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.soundAppear = SoundDef.Named("TradeWindow_Open");
            this.soundAmbient = SoundDef.Named("TradeWindow_Ambient");
            this.soundClose = SoundDef.Named("TradeWindow_Close");
            this.sorter1.name = TradeDialogSorterDefOf.Category.LabelCap;
            this.sorter1.ComparerSelector(this.sorter1.name);
            this.sorter2.name = TradeDialogSorterDefOf.MarketValue.LabelCap;
            this.sorter2.ComparerSelector(this.sorter2.name);
        }

        public override void PostOpen()
        {
            base.PostOpen();
            TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);
            if (TradeSession.colonyNegotiator.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking) < 0.99f)
            {
                Find.WindowStack.Add(Dialog_NodeTree.SimpleNotifyDialog("NegotiatorTalkingImpaired".Translate(new object[]
				{
					TradeSession.colonyNegotiator.LabelBaseShort
				})));
            }
            this.CacheTradeables();
        }

        public void CacheTradeables()
        {
            this.cachedCurrencyTradeable = (from x in TradeSession.deal.AllTradeables
                                            where x.IsCurrency
                                            select x).FirstOrDefault<Tradeable>();
            this.cachedTradeables = (from tr in TradeSession.deal.AllTradeables
                                     where tr.TraderWillTrade && !tr.IsCurrency
                                     select tr).OrderByDescending((Tradeable tr) => tr, this.sorter1.comparer).ThenBy((Tradeable tr) => tr, this.sorter2.comparer).ThenBy((Tradeable tr) => tr.ListOrderPriority).ThenBy((Tradeable tr) => tr.ThingDef.label).ThenBy(delegate(Tradeable tr)
                                     {
                                         QualityCategory result;
                                         if (tr.AnyThing.TryGetQuality(out result))
                                         {
                                             return (int)result;
                                         }
                                         return -1;
                                     }).ThenBy((Tradeable tr) => tr.AnyThing.HitPoints).ToList<Tradeable>();
        }

        public override void DoWindowContents(Rect inRect)
        {
            TradeSession.deal.UpdateCurrencyCount();
            Rect position = new Rect(0f, 0f, 350f, 27f);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Tiny;
            Rect rect = new Rect(0f, 0f, 60f, 27f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "SortBy".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(rect.xMax + 10f, 0f, 130f, 27f);
            if (Widgets.TextButton(rect2, this.sorter1.name, true, false))
            {
                this.OpenSorterChangeFloatMenu(0);
            }
            Rect rect3 = new Rect(rect2.xMax + 10f, 0f, 130f, 27f);
            if (Widgets.TextButton(rect3, this.sorter2.name, true, false))
            {
                this.OpenSorterChangeFloatMenu(1);
            }
            GUI.EndGroup();
            float num = inRect.width - 590f;
            Rect position2 = new Rect(num, 0f, inRect.width - num, 55f);
            GUI.BeginGroup(position2);
            Text.Font = GameFont.Medium;
            Rect rect4 = new Rect(0f, 0f, position2.width / 2f, position2.height);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect4, Find.Map.colonyInfo.ColonyName);
            Rect rect5 = new Rect(position2.width / 2f, 0f, position2.width / 2f, position2.height);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect5, TradeSession.tradeCompany.name);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            Text.Font = GameFont.Tiny;
            Rect rect6 = new Rect(position2.width / 2f - 100f - 30f, 0f, 200f, position2.height);
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(rect6, "DragToTrade".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            GUI.EndGroup();
            float num2 = 0f;
            if (this.cachedCurrencyTradeable != null)
            {
                float num3 = inRect.width - 16f;
                Rect rect7 = new Rect(0f, 55f, num3, 30f);
                TradeUI.DrawTradeableRow(rect7, this.cachedCurrencyTradeable, 1);
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(0f, 84f, num3);
                GUI.color = Color.white;
                num2 = 30f;
            }
            Rect mainRect = new Rect(0f, 55f + num2, inRect.width, inRect.height - 55f - 38f - num2 - 20f);
            this.FillMainRect(mainRect);
            Rect rect8 = new Rect(inRect.width / 2f - this.AcceptButtonSize.x / 2f, inRect.height - 55f, this.AcceptButtonSize.x, this.AcceptButtonSize.y);
            if (Widgets.TextButton(rect8, "AcceptButton".Translate(), true, false))
            {
                Action action = delegate
                {
                    //bool flag;
                    if (TradeSession.deal.TryMakeDeal())
                    {
                        //if (flag)
                        {
                            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
                            this.Close(false);
                        }
                        //else
                        {
                            this.Close(true);
                        }
                    }
                };
                if (TradeSession.deal.DoesTraderHaveEnoughSilver())
                {
                    action();
                }
                else
                {
                    this.FlashSilver();
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    Find.WindowStack.Add(new Dialog_Confirm("ConfirmTraderShortFunds".Translate(), action, false));
                }
                Event.current.Use();
            }
            Rect rect9 = new Rect(rect8.x - 10f - this.OtherBottomButtonSize.x, rect8.y, this.OtherBottomButtonSize.x, this.OtherBottomButtonSize.y);
            if (Widgets.TextButton(rect9, "ResetButton".Translate(), true, false))
            {
                SoundDefOf.TickLow.PlayOneShotOnCamera();
                TradeSession.deal.Reset();
                this.CacheTradeables();
                Event.current.Use();
            }
            Rect rect10 = new Rect(rect8.xMax + 10f, rect8.y, this.OtherBottomButtonSize.x, this.OtherBottomButtonSize.y);
            if (Widgets.TextButton(rect10, "CancelButton".Translate(), true, false))
            {
                this.Close(true);
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
            float height = 6f + (float)this.cachedTradeables.Count * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect);
            float num = 6f;
            float num2 = this.scrollPosition.y - 30f;
            float num3 = this.scrollPosition.y + mainRect.height;
            int num4 = 0;
            for (int i = 0; i < this.cachedTradeables.Count; i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawTradeableRow(rect, this.cachedTradeables[i], num4);
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
            float num = rect.width;
            int num2 = trad.CountHeldBy(Transactor.Trader);
            if (num2 != 0)
            {
                Rect rect2 = new Rect(num - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                }
                Text.Anchor = TextAnchor.MiddleRight;
                Rect rect3 = rect2;
                rect3.xMin += 5f;
                rect3.xMax -= 5f;
                Widgets.Label(rect3, num2.ToStringCached());
                TooltipHandler.TipRegion(rect2, "TraderCount".Translate());
                Rect rect4 = new Rect(rect2.x - 100f, 0f, 100f, rect.height);
                Text.Anchor = TextAnchor.MiddleRight;
                TradeUI.DrawPrice(rect4, trad, TradeAction.ToDrop);
            }
            num -= 175f;
            Rect rect5 = new Rect(num - 240f, 0f, 240f, rect.height);
            TradeUI.DrawCountAdjustInterface(rect5, trad);
            num -= 240f;
            int num3 = trad.CountHeldBy(Transactor.Colony);
            if (num3 != 0)
            {
                Rect rect6 = new Rect(num - 100f, 0f, 100f, rect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                TradeUI.DrawPrice(rect6, trad, TradeAction.ToLaunch);
                Rect rect7 = new Rect(rect6.x - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect7))
                {
                    Widgets.DrawHighlight(rect7);
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect rect8 = rect7;
                rect8.xMin += 5f;
                rect8.xMax -= 5f;
                Widgets.Label(rect8, num3.ToStringCached());
                TooltipHandler.TipRegion(rect7, "ColonyCount".Translate());
            }
            num -= 175f;
            Rect rect9 = new Rect(0f, 0f, num, rect.height);
            if (Mouse.IsOver(rect9))
            {
                Widgets.DrawHighlight(rect9);
            }
            Rect rect10 = new Rect(0f, 0f, 27f, 27f);
            Widgets.ThingIcon(rect10, trad.AnyThing);
            Widgets.InfoCardButton(40f, 0f, trad.AnyThing);
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect11 = new Rect(80f, 0f, rect9.width - 80f, rect.height);
            Text.WordWrap = false;
            Widgets.Label(rect11, trad.Label);
            Text.WordWrap = true;
            Tradeable localTrad = trad;
            TooltipHandler.TipRegion(rect9, new TipSignal(() => localTrad.Label + ": " + localTrad.TipDescription, localTrad.GetHashCode()));
            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        public void OpenSorterChangeFloatMenu(int sorterIndex)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<TradeDialogSorterDef> allDefsListForReading = DefDatabase<TradeDialogSorterDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                TradeDialogSorterDef def = allDefsListForReading[i];
                list.Add(new FloatMenuOption(def.LabelCap, delegate
                {
                    if (sorterIndex == 0)
                    {
                        this.sorter1.name = def.LabelCap;
                        this.sorter1.ComparerSelector(this.sorter1.name);
                    }
                    else
                    {
                        this.sorter2.name = def.LabelCap;
                        this.sorter2.ComparerSelector(this.sorter2.name);
                    }
                    this.CacheTradeables();
                }, MenuOptionPriority.Medium, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list, false));
        }

        public void FlashSilver()
        {
            Dialog_Trade.lastCurrencyFlashTime = Time.time;
        }
    }
}
