using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Pawn : Pawn
    {
        public new IEnumerable<Thing> ColonyThingsWillingToBuy => TradeUtil.AllSellableThings;

        // trader.Goods still holds list of caravan chattel pawns, when all items been already transfered to the trade center
        public new IEnumerable<Thing> Goods => Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>()
            .FirstOrDefault(tradeCenter => tradeCenter.trader == this)
            .traderStock.Concat(trader.Goods);

        // changed butcher yields
        public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
        {
            //Return additional butcher products
            foreach (var t in base.ButcherProducts(butcher, efficiency))
                yield return t;

            //Return available sheer products
            var compShearable = this.TryGetComp<CompShearable>();
            if (compShearable != null && compShearable.ActiveAndFull)
            {
                var woolAmount = compShearable.Props.woolAmount;
                while (woolAmount > 0)
                {
                    var num = Mathf.Clamp(woolAmount, 1, compShearable.Props.woolDef.stackLimit);
                    woolAmount -= num;
                    var thing = ThingMaker.MakeThing(compShearable.Props.woolDef);
                    thing.stackCount = num;
                    yield return thing;
                }
            }

            //Return meat
            var meatCount = GenMath.RoundRandom(this.GetStatValue(StatDefOf.MeatAmount)*efficiency);
            if (meatCount > 0)
            {
                var meat = ThingMaker.MakeThing(RaceProps.Humanlike
                    ? DefDatabase<ThingDef>.GetNamedSilentFail("MeatHumanRaw")
                    : RaceProps.baseBodySize < 0.7f
                        ? DefDatabase<ThingDef>.GetNamedSilentFail("MeatSmallRaw")
                        : DefDatabase<ThingDef>.GetNamedSilentFail("MeatRaw"));
                meat.stackCount = meatCount;
                yield return meat;
            }

            //Return leather
            var leatherCount = GenMath.RoundRandom(this.GetStatValue(StatDefOf.LeatherAmount)*efficiency);
            if (leatherCount > 0)
            {
                var leather = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamedSilentFail("Leather"));
                leather.stackCount = leatherCount;
                yield return leather;
            }

            //Return bones
            var bonesCount = GenMath.RoundRandom(this.GetStatValue(StatDef.Named("BoneAmount"))*efficiency);
            if (bonesCount > 0)
            {
                var bones = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamedSilentFail("Bone"));
                bones.stackCount = bonesCount;
                yield return bones;
            }

            //Return tallow
            var tallowCount = GenMath.RoundRandom(this.GetStatValue(StatDef.Named("TallowAmount"))*efficiency);
            if (tallowCount > 0)
            {
                var tallow = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamedSilentFail("Tallow"));
                tallow.stackCount = tallowCount;
                yield return tallow;
            }
        }
    }
}