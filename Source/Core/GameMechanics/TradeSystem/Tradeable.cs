using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class Tradeable
    {
        public List<Thing> thingsColony = new List<Thing>();
        public List<Thing> thingsTrader = new List<Thing>();
        public int offerCount;

        public static readonly SimpleCurve LaunchPricePostFactorCurve = new SimpleCurve
		{
			new CurvePoint(2000f, 1f),
			new CurvePoint(12000f, 0.5f),
			new CurvePoint(200000f, 0.2f)
		};

        public Thing FirstThingColony
        {
            get
            {
                if (thingsColony.Count == 0)
                {
                    return null;
                }
                return thingsColony[0];
            }
        }

        public Thing FirstThingTrader
        {
            get
            {
                if (thingsTrader.Count == 0)
                {
                    return null;
                }
                return thingsTrader[0];
            }
        }

        public virtual string Label
        {
            get
            {
                return AnyThing.LabelBase.CapitalizeFirst();
            }
        }

        public virtual float BaseMarketValue
        {
            get
            {
                return AnyThing.MarketValue;
            }
        }

        public virtual float ListOrderPriority
        {
            get
            {
                int num;
                if (IsCurrency)
                {
                    num = 100;
                }
                else if (ThingDef == ThingDefOf.Gold)
                {
                    num = 99;
                }
                else if (ThingDef.Minifiable)
                {
                    num = 90;
                }
                else if (ThingDef.IsApparel)
                {
                    num = 80;
                }
                else if (ThingDef.IsRangedWeapon)
                {
                    num = 70;
                }
                else if (ThingDef.IsMeleeWeapon)
                {
                    num = 60;
                }
                else if (ThingDef.isBodyPartOrImplant)
                {
                    num = 50;
                }
                else if (ThingDef.CountAsResource)
                {
                    num = -10;
                }
                else
                {
                    num = 20;
                }
                return (float)num;
            }
        }

        public bool TraderWillTrade
        {
            get
            {
                return TradeSession.tradeCompany.def.WillTrade(AnyThing);
            }
        }

        public Thing AnyThing
        {
            get
            {
                if (FirstThingColony != null)
                {
                    return FirstThingColony.GetInnerIfMinified();
                }
                if (FirstThingTrader != null)
                {
                    return FirstThingTrader.GetInnerIfMinified();
                }
                Log.Error(base.GetType() + " lacks AnyThing.");
                return null;
            }
        }

        public virtual ThingDef ThingDef
        {
            get
            {
                return AnyThing.def;
            }
        }

        public ThingDef StuffDef
        {
            get
            {
                return AnyThing.Stuff;
            }
        }

        public virtual string TipDescription
        {
            get
            {
                return ThingDef.description;
            }
        }

        public TradeAction ActionToDo
        {
            get
            {
                if (offerCount == 0)
                {
                    return TradeAction.None;
                }
                if (offerCount > 0)
                {
                    return TradeAction.ToDrop;
                }
                return TradeAction.ToLaunch;
            }
        }

        public bool IsCurrency
        {
            get
            {
                return !Bugged && ThingDef == ThingDefOf.Silver;
            }
        }

        public float CurTotalSilverCost
        {
            get
            {
                if (ActionToDo == TradeAction.None)
                {
                    return 0f;
                }
                return (float)offerCount * PriceFor(ActionToDo);
            }
        }

        public virtual Window NewInfoDialog
        {
            get
            {
                return new Dialog_InfoCard(ThingDef);
            }
        }

        public bool Bugged
        {
            get
            {
                return AnyThing == null;
            }
        }

        public Tradeable()
        {
        }

        public Tradeable(Thing thingColony, Thing thingTrader)
        {
            thingsColony.Add(thingColony);
            thingsTrader.Add(thingTrader);
        }

        public void AddThing(Thing t, Transactor trans)
        {
            if (trans == Transactor.Colony)
            {
                thingsColony.Add(t);
            }
            if (trans == Transactor.Trader)
            {
                thingsTrader.Add(t);
            }
        }

        public AcceptanceReport CanSetToDropOneMore()
        {
            if (Bugged)
            {
                return false;
            }
            if (!TraderWillTrade)
            {
                return new AcceptanceReport("TraderWillNotTrade".Translate());
            }
            if (CountPostDealFor(Transactor.Trader) <= 0)
            {
                return new AcceptanceReport("TraderHasNoMore".Translate());
            }
            return true;
        }

        public AcceptanceReport TrySetToDropOneMore()
        {
            if (Bugged)
            {
                return false;
            }
            if (IsCurrency)
            {
                Log.Error("Should not increment currency tradeable " + this);
                return false;
            }
            AcceptanceReport result = CanSetToDropOneMore();
            if (!result.Accepted)
            {
                return result;
            }
            offerCount++;
            return true;
        }

        public void SetToDropMax()
        {
            offerCount = CountHeldBy(Transactor.Trader);
        }

        public AcceptanceReport CanSetToLaunchOneMore()
        {
            if (Bugged)
            {
                return false;
            }
            if (!TraderWillTrade)
            {
                return new AcceptanceReport("TraderWillNotTrade".Translate());
            }
            if (CountPostDealFor(Transactor.Colony) <= 0)
            {
                return new AcceptanceReport("ColonyHasNoMore".Translate());
            }
            return true;
        }

        public AcceptanceReport TrySetToLaunchOneMore()
        {
            if (Bugged)
            {
                return false;
            }
            if (IsCurrency)
            {
                Log.Error("Should not decrement currency tradeable " + this);
                return false;
            }
            AcceptanceReport result = CanSetToLaunchOneMore();
            if (!result.Accepted)
            {
                return result;
            }
            offerCount--;
            return true;
        }

        public void SetToLaunchMax()
        {
            offerCount = -CountHeldBy(Transactor.Colony);
        }

        public List<Thing> TransactorThings(Transactor trans)
        {
            if (trans == Transactor.Colony)
            {
                return thingsColony;
            }
            return thingsTrader;
        }

        public int CountHeldBy(Transactor trans)
        {
            List<Thing> list = TransactorThings(trans);
            int num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                num += list[i].stackCount;
            }
            return num;
        }

        public int CountPostDealFor(Transactor trans)
        {
            if (trans == Transactor.Colony)
            {
                return CountHeldBy(trans) + offerCount;
            }
            return CountHeldBy(trans) - offerCount;
        }

        public float PriceFor(TradeAction action)
        {
            float num = TradeSession.tradeCompany.def.PriceTypeFor(ThingDef, action).PriceMultiplier();
            float num2 = TradeSession.tradeCompany.RandomPriceFactorFor(this);
            float num3 = 1f;
            if (TradeSession.colonyNegotiator != null)
            {
                float num4 = Mathf.Clamp01(TradeSession.colonyNegotiator.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking));
                if (action == TradeAction.ToDrop)
                {
                    num3 += 1f - num4;
                }
                else
                {
                    num3 -= 0.5f * (1f - num4);
                }
            }
            float num5;
            if (action == TradeAction.ToDrop)
            {
                num5 = BaseMarketValue * (1f - TradeSession.colonyNegotiator.GetStatValue(StatDefOf.TradePriceImprovement, true)) * num3 * num * num2;
                num5 = Mathf.Max(num5, 0.5f);
            }
            else
            {
                num5 = BaseMarketValue * Find.Storyteller.difficulty.baseSellPriceFactor * AnyThing.GetStatValue(StatDefOf.SellPriceFactor, true) * (1f + TradeSession.colonyNegotiator.GetStatValue(StatDefOf.TradePriceImprovement, true)) * num3 * num * num2;
                num5 *= Tradeable.LaunchPricePostFactorCurve.Evaluate(num5);
                num5 = Mathf.Max(num5, 0.01f);
                if (num5 >= PriceFor(TradeAction.ToDrop))
                {
                    Log.ErrorOnce("Skill of negotitator trying to put sell price above buy price.", 65387);
                    num5 = PriceFor(TradeAction.ToDrop);
                }
            }
            if (num5 > 99.5f)
            {
                num5 = Mathf.Round(num5);
            }
            return num5;
        }

        public virtual void ResolveTrade()
        {
            // goods offered by colony
            if (ActionToDo == TradeAction.ToLaunch)
            {
                if (thingsColony.Count == 0)
                {
                    Log.Error("Nothing left to give to trader for " + this);
                    return;
                }
                // thingsColony used as a stack of the same type things. We always deal with the first thing in this stack
                Thing thing = FirstThingColony;
                // if thing is a stackable resource
                if (thing.def.stackLimit > 1)
                {
                    WorkGiver_HaulToTrade.requestedResourceCounters.Add(new ThingCount(thing.def, Math.Abs(offerCount)));
                    WorkGiver_HaulToTrade.FindOccupiedTradingPost().offeredResourceCounters.Add(new ThingCount(thing.def, Math.Abs(offerCount)));
                }
                else
                {
                    // clears placed blueprints for minified things
                    thing.Launched();
                    WorkGiver_HaulToTrade.requestedItems.Add(thing);
                    WorkGiver_HaulToTrade.FindOccupiedTradingPost().offeredItems.Add(thing);
                }

                thingsColony.Clear();
            }
            // goods offered by merchant
            else if (ActionToDo == TradeAction.ToDrop)
            {
                // offerLeftover - current amount of tradeable type to offer
                // offerCount - total amount of tradeable type to offer
                int offerLeftover = Math.Abs(offerCount);
                while (offerLeftover > 0)
                {
                    if (thingsTrader.Count == 0)
                    {
                        Log.Error("Nothing left to take from trader for " + this);
                        return;
                    }
                    Thing currentThing = FirstThingTrader;
                    int transferCount = Mathf.Min(offerLeftover, currentThing.stackCount);
                    // make new thing\stack by splitting required amount of another stack (can take it whole)
                    Thing transferedThing = currentThing.SplitOff(transferCount);
                    offerLeftover -= transferCount;
                    if (transferedThing == currentThing)
                    {
                        // remove thing from tradeCompany stock
                        TradeSession.tradeCompany.RemoveFromStock(currentThing);
                        // remove thing from current tradeable list
                        thingsTrader.Remove(currentThing);
                    }
                    // update current trade deal merchant offer
                    WorkGiver_HaulToTrade.FindOccupiedTradingPost().merchantOffer.Add(transferedThing);
                }
            }
        }

        public static void DropThing(Thing t)
        {
            IntVec3 c = DropCellFinder.TradeDropSpot();
            DropPodUtility.MakeDropPodAt(c, new DropPodInfo
            {
                SingleContainedThing = t,
                leaveSlag = false
            });
        }

        public override string ToString()
        {
            return string.Concat(new object[]
			{
				base.GetType(),
				"(",
				ThingDef,
				", countToDrop=",
				offerCount,
				")"
			});
        }

        public override int GetHashCode()
        {
            return AnyThing.GetHashCode();
        }
    }
}