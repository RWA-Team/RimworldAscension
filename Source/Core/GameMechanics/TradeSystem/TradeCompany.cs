using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

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
            name = RulePackDef.Named("NamerTraderGeneral").GenerateDefault_Name(
                from vis in Find.PassingShipManager.passingShips 
                select vis.name);
            randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
        }

		public override string FullTitle => name + " (" + def.label + ")";

        public int Silver => CountHeldOf(ThingDefOf.Silver);

        public override void TryOpenComms(Pawn negotiator)
		{
			if (TradeSession.tradeCompany == null)
				return;

			TradeSession.InitiateTradeDeal(negotiator);
			Find.WindowStack.Add(new Dialog_Trade());
		}

		public override string GetCallLabel()
		{
			return name + " (" + def.label + ")";
		}

		public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
		{
			var thing = HeldThingMatching(thingDef, stuffDef);
			if (thing != null)
			{
				return thing.stackCount;
			}
			return 0;
		}

		public void AddToStock(Thing thing)
		{
			var thing2 = HeldThingMatching(thing);
			if (thing2 != null)
			{
				thing2.stackCount += thing.stackCount;
			}
			else
			{
                things.Add(thing);
			}
		}

		public Thing HeldThingMatching(Thing thing)
		{
			if (thing is Pawn)
			{
				return null;
			}
		    return things.FirstOrDefault(thing2 => TradeUtility.TradeAsOne(thing2, thing));
		}

		public void RemoveFromStock(Thing thing)
		{
			if (!things.Contains(thing))
			{
				Log.Error(string.Concat("Tried to remove ", thing, " from trader ", name, " who didn't have it."));
				return;
			}
            things.Remove(thing);
		}

		public Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
		{
		    return things.FirstOrDefault(t => t.def == thingDef && t.Stuff == stuffDef);
		}

        public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
		{
			var thing = HeldThingMatching(thingDef, stuffDef);
			if (thing == null)
			{
				Log.Error("Changing count of thing trader doesn't have: " + thingDef);
			}
			thing.stackCount += count;
		}

		public float RandomPriceFactorFor(Tradeable tradeable)
		{
			var num = DefDatabase<ThingDef>.AllDefsListForReading.IndexOf(tradeable.ThingDef);
			Rand.PushSeed();
			Rand.Seed = randomPriceFactorSeed * num;
			var result = Rand.Range(0.9f, 1.1f);
			Rand.PopSeed();
			return result;
		}
        public override string ToString()
        {
            return FullTitle;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.LookDef(ref def, "def");
            Scribe_Collections.LookList(ref things, "things", LookMode.Deep);
            Scribe_Values.LookValue(ref randomPriceFactorSeed, "randomPriceFactorSeed");
        }
	}
}