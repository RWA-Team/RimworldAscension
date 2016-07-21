using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Tradeable : Tradeable
    {
        public TradeCenter tradeCenter;

        public new float PriceFor(TradeAction action)
        {
            if (tradeCenter == null) tradeCenter = TradeUtil.FindOccupiedTradeCenter(TradeSession.playerNegotiator);
            return tradeCenter.ThingTypeFinalCost(AnyThing, action, 1);
        }

        public new virtual void ResolveTrade()
        {
            if (tradeCenter == null) tradeCenter = TradeUtil.FindOccupiedTradeCenter(TradeSession.playerNegotiator);
            // goods offered by colony
            if (ActionToDo == TradeAction.PlayerSells)
            {
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
                    currentThing.PreTraded(TradeAction.PlayerBuys, tradeCenter.negotiator, tradeCenter.trader);
                    tradeCenter.pendingItemsCounter.Add(currentThing);
                    thingsColony.Remove(currentThing);
                }
            }
            // goods offered by trader
            else if (ActionToDo == TradeAction.PlayerBuys)
            {
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
                    // trasfer sellable to the trader exchange container
                    tradeCenter.traderStock.TransferToContainer(transferedThing, tradeCenter.traderExchangeContainer, transferedThing.stackCount);
                    // update trader goods total cost
                    tradeCenter.traderGoodsCost += tradeCenter.ThingTypeFinalCost(transferedThing, TradeAction.PlayerBuys);
                    CheckTeachOpportunity(transferedThing);
                }
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