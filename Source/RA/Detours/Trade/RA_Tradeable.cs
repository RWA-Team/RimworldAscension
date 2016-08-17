using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Tradeable : Tradeable
    {
        public TradeCenter tradeCenter;

        public override void ResolveTrade()
        {
            tradeCenter = TradeUtil.FindOccupiedTradeCenter(TradeSession.playerNegotiator);

            // goods offered by colony
            switch (ActionToDo)
            {
                case TradeAction.PlayerSells:
                    // offerLeftover - current amount of tradeable type to offer
                    var exportLeftover = Math.Abs(countToDrop);
                    while (exportLeftover > 0)
                    {
                        if (thingsColony.Count == 0)
                        {
                            Log.Error("Nothing left to give to trader for " + this);
                            return;
                        }
                        // thingsColony used as a stack of the same type things. We always deal with the first thing in this stack
                        var currentThing = FirstThingColony;
                        var offerCount = Mathf.Min(exportLeftover, currentThing.stackCount);
                        exportLeftover -= offerCount;
                        currentThing.SetForbidden(true);
                        // if thing is a stackable resource
                        if (currentThing.def.stackLimit > 1)
                        {
                            // make counter for deal resolve (control group)
                            if (tradeCenter.pendingResourcesCounters.ContainsKey(ThingDef))
                                tradeCenter.pendingResourcesCounters[ThingDef] += offerCount;
                            else
                                tradeCenter.pendingResourcesCounters.Add(currentThing.def, offerCount);
                        }
                        // clears placed blueprints for minified things
                        currentThing.PreTraded(ActionToDo, tradeCenter.negotiator, tradeCenter.trader);
                        tradeCenter.pendingItemsCounter.Add(currentThing);
                        thingsColony.Remove(currentThing);
                    }
                    break;
                case TradeAction.PlayerBuys:
                    // offerLeftover - current amount of tradeable type to offer
                    var importLeftover = Math.Abs(countToDrop);
                    while (importLeftover > 0)
                    {
                        if (thingsTrader.Count == 0)
                        {
                            Log.Error("Nothing left to take from trader for " + this);
                            return;
                        }
                        var currentThing = FirstThingTrader;
                        var transferCount = Mathf.Min(importLeftover, currentThing.stackCount);
                        // make new thing\stack by splitting required amount of another stack (can take it whole)
                        var transferedThing = currentThing.SplitOff(transferCount);
                        importLeftover -= transferCount;
                        // check for full thing transfer
                        if (transferedThing == currentThing)
                        {
                            // remove thing from current tradeable list
                            thingsTrader.Remove(currentThing);
                        }
                        transferedThing.PreTraded(TradeAction.PlayerBuys, tradeCenter.negotiator, tradeCenter.trader);
                        // trasfer sellable to the trader exchange container
                        tradeCenter.traderStock.TransferToContainer(transferedThing, tradeCenter.traderExchangeContainer, transferedThing.stackCount);
                        // update trader goods total cost
                        tradeCenter.traderGoodsCost += new Tradeable(transferedThing, transferedThing).PriceFor(ActionToDo);
                        CheckTeachOpportunity(transferedThing);
                    }
                    break;
            }
        }

        public void CheckTeachOpportunity(Thing boughtThing)
        {
            var building = boughtThing as Building;
            if (building == null)
            {
                var minifiedThing = boughtThing as MinifiedThing;
                if (minifiedThing != null)
                {
                    building = minifiedThing.InnerThing as Building;
                }
            }
            if (building?.def.building?.boughtConceptLearnOpportunity != null)
            {
                ConceptDecider.TeachOpportunity(building.def.building.boughtConceptLearnOpportunity,
                    OpportunityType.GoodToKnow);
            }
        }
    }
}