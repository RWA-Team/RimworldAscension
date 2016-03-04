using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class TradeDeal
    {
        public List<Tradeable> tradeables = new List<Tradeable>();
        public bool isPending;

        public int TradeableCount => tradeables.Count;

        public Tradeable SilverTradeable
        {
            get
            { return tradeables.FirstOrDefault(t => t.ThingDef == ThingDefOf.Silver); }
        }

        public IEnumerable<Tradeable> AllTradeables => tradeables;

        public TradeDeal()
        {
            isPending = false;
            Reset();
        }

        public IEnumerator<Tradeable> GetEnumerator()
        {
            return tradeables.GetEnumerator();
        }

        public void Reset()
        {
            tradeables.Clear();
            AddAllTradeables();
        }

        public void AddAllTradeables()
        {
            foreach (var current in TradeUtility.AllSellableThings)
            {
                AddToTradeables(current, Transactor.Colony);
            }
            foreach (var current in Find.ListerPawns.PrisonersOfColony)
            {
                if (current.guest.PrisonerIsSecure && current.SpawnedInWorld)
                {
                    AddToTradeables(current, Transactor.Colony);
                }
            }
            foreach (var current in from pa in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                                     where pa.RaceProps.Animal
                                     select pa)
            {
                if (current.HostFaction == null && !current.Broken && !current.Downed)
                {
                    AddToTradeables(current, Transactor.Colony);
                }
            }
            foreach (var current in TradeSession.tradeCompany.things)
            {
                AddToTradeables(current, Transactor.Trader);
            }
        }

        public void AddToTradeables(Thing t, Transactor trans)
        {
            var tradeable = TradeableMatching(t);
            if (tradeable == null)
            {
                var pawn = t as Pawn;
                tradeable = pawn != null ? new Tradeable_Pawn() : new Tradeable();
                tradeables.Add(tradeable);
            }
            tradeable.AddThing(t, trans);
        }

        public Tradeable TradeableMatching(Thing thing)
        {
            return tradeables.FirstOrDefault(current => TradeUtility.TradeAsOne(thing, current.AnyThing));
        }

        public void UpdateCurrencyCount()
        {
            var num = tradeables.Where(current => !current.IsCurrency).Sum(current => current.CurTotalSilverCost);
            SilverTradeable.offerCount = -Mathf.RoundToInt(num);
        }

        public bool TryMakeDeal()
        {
            if (SilverTradeable.CountPostDealFor(Transactor.Colony) < 0)
            {
                Find.WindowStack.WindowOfType<Dialog_Trade>().FlashSilver();
                Messages.Message("MessageColonyCannotAfford".Translate(), MessageSound.RejectInput);
                return false;
            }
            UpdateCurrencyCount();
            LimitCurrencyCountToTraderFunds();
            foreach (var current in tradeables)
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
            return SilverTradeable.CountPostDealFor(Transactor.Trader) >= 0;
        }

        public void LimitCurrencyCountToTraderFunds()
        {
            if (SilverTradeable.offerCount > SilverTradeable.CountHeldBy(Transactor.Trader))
            {
                SilverTradeable.offerCount = SilverTradeable.CountHeldBy(Transactor.Trader);
            }
        }
    }
}