using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class TradeCompany : PassingShip
	{
        public TraderKindDef def;
		public List<Thing> things = new List<Thing>();
        public int randomPriceFactorSeed;

        public TradeCompany(TraderKindDef def)
        {
            this.def = def;
            this.name = DefDatabase<RulePackDef>.GetNamed("NamerTraderGeneral", true).GenerateDefault_Name(
                from vis in Find.PassingShipManager.passingShips 
                select vis.name);
            this.randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
        }

		public override string FullTitle
		{
			get
			{
				return this.name + " (" + this.def.label + ")";
			}
		}

		public int Silver
		{
			get
			{
				return this.CountHeldOf(ThingDefOf.Silver, null);
			}
		}

		public override void TryOpenComms(Pawn negotiator)
		{
			if (TradeSession.tradeCompany == null)
				return;

			TradeSession.InitiateTradeDeal(negotiator);
			Find.WindowStack.Add(new Dialog_Trade());
		}

		public override string GetCallLabel()
		{
			return this.name + " (" + this.def.label + ")";
		}

		public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing != null)
			{
				return thing.stackCount;
			}
			return 0;
		}

		public void AddToStock(Thing thing)
		{
			Thing thing2 = this.HeldThingMatching(thing);
			if (thing2 != null)
			{
				thing2.stackCount += thing.stackCount;
			}
			else
			{
				this.things.Add(thing);
			}
		}

		public Thing HeldThingMatching(Thing thing)
		{
			if (thing is Pawn)
			{
				return null;
			}
			for (int i = 0; i < this.things.Count; i++)
			{
				Thing thing2 = this.things[i];
				if (TradeUtility.TradeAsOne(thing2, thing))
				{
					return thing2;
				}
			}
			return null;
		}

		public void RemoveFromStock(Thing thing)
		{
			if (!this.things.Contains(thing))
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to remove ",
					thing,
					" from trader ",
					this.name,
					" who didn't have it."
				}));
				return;
			}
			this.things.Remove(thing);
		}

		public Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
		{
			for (int i = 0; i < this.things.Count; i++)
			{
				if (this.things[i].def == thingDef && this.things[i].Stuff == stuffDef)
				{
					return this.things[i];
				}
			}
			return null;
		}

		public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing == null)
			{
				Log.Error("Changing count of thing trader doesn't have: " + thingDef);
			}
			thing.stackCount += count;
		}

		public float RandomPriceFactorFor(Tradeable tradeable)
		{
			int num = DefDatabase<ThingDef>.AllDefsListForReading.IndexOf(tradeable.ThingDef);
			Rand.PushSeed();
			Rand.Seed = this.randomPriceFactorSeed * num;
			float result = Rand.Range(0.9f, 1.1f);
			Rand.PopSeed();
			return result;
		}
        public override string ToString()
        {
            return this.FullTitle;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.LookDef<TraderKindDef>(ref this.def, "def");
            Scribe_Collections.LookList<Thing>(ref this.things, "things", LookMode.Deep, new object[0]);
            Scribe_Values.LookValue<int>(ref this.randomPriceFactorSeed, "randomPriceFactorSeed", 0, false);
        }
	}
}
