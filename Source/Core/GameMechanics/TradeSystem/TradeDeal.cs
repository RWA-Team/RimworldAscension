using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimworldAscension
{
    public class TradeDeal
    {
        public List<Tradeable> tradeables = new List<Tradeable>();
        public bool isPending;

        public int TradeableCount
        {
            get
            {
                return this.tradeables.Count;
            }
        }

        public Tradeable SilverTradeable
        {
            get
            {
                for (int i = 0; i < this.tradeables.Count; i++)
                {
                    if (this.tradeables[i].ThingDef == ThingDefOf.Silver)
                    {
                        return this.tradeables[i];
                    }
                }
                return null;
            }
        }

        public IEnumerable<Tradeable> AllTradeables
        {
            get
            {
                return this.tradeables;
            }
        }

        public TradeDeal()
        {
            isPending = false;
            this.Reset();
        }

        public IEnumerator<Tradeable> GetEnumerator()
        {
            return this.tradeables.GetEnumerator();
        }

        public void Reset()
        {
            this.tradeables.Clear();
            this.AddAllTradeables();
        }

        public void AddAllTradeables()
        {
            foreach (Thing current in TradeUtility.AllSellableThings)
            {
                this.AddToTradeables(current, Transactor.Colony);
            }
            foreach (Pawn current in Find.ListerPawns.PrisonersOfColony)
            {
                if (current.guest.PrisonerIsSecure && current.SpawnedInWorld)
                {
                    this.AddToTradeables(current, Transactor.Colony);
                }
            }
            foreach (Pawn current in from pa in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                                     where pa.RaceProps.Animal
                                     select pa)
            {
                if (current.HostFaction == null && !current.Broken && !current.Downed)
                {
                    this.AddToTradeables(current, Transactor.Colony);
                }
            }
            foreach (Thing current in TradeSession.tradeCompany.things)
            {
                this.AddToTradeables(current, Transactor.Trader);
            }
        }

        public void AddToTradeables(Thing t, Transactor trans)
        {
            Tradeable tradeable = this.TradeableMatching(t);
            if (tradeable == null)
            {
                Pawn pawn = t as Pawn;
                if (pawn != null)
                {
                    tradeable = new Tradeable_Pawn();
                }
                else
                {
                    tradeable = new Tradeable();
                }
                this.tradeables.Add(tradeable);
            }
            tradeable.AddThing(t, trans);
        }

        public Tradeable TradeableMatching(Thing thing)
        {
            foreach (Tradeable current in this.tradeables)
            {
                if (TradeUtility.TradeAsOne(thing, current.AnyThing))
                {
                    return current;
                }
            }
            return null;
        }

        public void UpdateCurrencyCount()
        {
            float num = 0f;
            foreach (Tradeable current in this.tradeables)
            {
                if (!current.IsCurrency)
                {
                    num += current.CurTotalSilverCost;
                }
            }
            this.SilverTradeable.offerCount = -Mathf.RoundToInt(num);
        }

        public bool TryMakeDeal()
        {
            if (this.SilverTradeable.CountPostDealFor(Transactor.Colony) < 0)
            {
                Find.WindowStack.WindowOfType<Dialog_Trade>().FlashSilver();
                Messages.Message("MessageColonyCannotAfford".Translate(), MessageSound.RejectInput);
                return false;
            }
            this.UpdateCurrencyCount();
            this.LimitCurrencyCountToTraderFunds();
            foreach (Tradeable current in this.tradeables)
            {
                if (current.ActionToDo != TradeAction.None)
                {
                    isPending = true;
                    current.ResolveTrade();
                }
            }
            //this.Reset();
            return true;
        }

        public bool DoesTraderHaveEnoughSilver()
        {
            return this.SilverTradeable.CountPostDealFor(Transactor.Trader) >= 0;
        }

        public void LimitCurrencyCountToTraderFunds()
        {
            if (this.SilverTradeable.offerCount > this.SilverTradeable.CountHeldBy(Transactor.Trader))
            {
                this.SilverTradeable.offerCount = this.SilverTradeable.CountHeldBy(Transactor.Trader);
            }
        }
    }
}