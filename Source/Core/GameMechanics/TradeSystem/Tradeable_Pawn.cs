using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimworldAscension
{
    public class Tradeable_Pawn : Tradeable
    {
        public override Window NewInfoDialog
        {
            get
            {
                return new Dialog_InfoCard(this.AnyPawn);
            }
        }

        public override string Label
        {
            get
            {
                string text = base.Label;
                if (this.AnyPawn.Name != null && !this.AnyPawn.Name.Numerical)
                {
                    text = text + ", " + this.AnyPawn.def.label;
                }
                string text2 = text;
                return string.Concat(new string[]
				{
					text2,
					" (",
					this.AnyPawn.gender.GetLabel(),
					", ",
					this.AnyPawn.ageTracker.AgeBiologicalYearsFloat.ToString("F0"),
					")"
				});
            }
        }

        public override string TipDescription
        {
            get
            {
                string str = this.AnyPawn.MainDesc(true);
                return str + "\n\n" + this.AnyPawn.def.description;
            }
        }

        public Pawn AnyPawn
        {
            get
            {
                return (Pawn)base.AnyThing;
            }
        }

        public override void ResolveTrade()
        {
            if (base.ActionToDo == TradeAction.ToLaunch)
            {
                List<Pawn> list = this.thingsColony.Take(-this.offerCount).Cast<Pawn>().ToList<Pawn>();
                for (int i = 0; i < list.Count; i++)
                {
                    Pawn pawn = list[i];
                    pawn.PreSold();
                    pawn.DeSpawn();
                    if (!pawn.RaceProps.Humanlike)
                    {
                        TradeSession.tradeCompany.AddToStock(pawn);
                    }
                    else
                    {
                        foreach (Pawn current in Find.ListerPawns.ColonistsAndPrisoners)
                        {
                            current.needs.mood.thoughts.TryGainThought(ThoughtDefOf.KnowPrisonerSold);
                        }
                    }
                }
            }
            else if (base.ActionToDo == TradeAction.ToDrop)
            {
                List<Pawn> list2 = this.thingsTrader.Take(this.offerCount).Cast<Pawn>().ToList<Pawn>();
                for (int j = 0; j < list2.Count; j++)
                {
                    Pawn pawn2 = list2[j];
                    TradeSession.tradeCompany.RemoveFromStock(pawn2);
                    Tradeable.DropThing(pawn2);
                    pawn2.SetFaction(Faction.OfColony, null);
                }
            }
        }
    }
}
