using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public static class Diplomacy
    {
        public const float MinRelationsToCommunicate = -70f;
        public const float MinRelationsFriendly = 40f;
        public const int GiftSilverAmount = 300;
        public const float GiftSilverGoodwillChange = 12f;
        public const float MilitaryAidRelsChange = -25f;

        public static DiaNode root;
        public static Pawn negotiator;
        public static Faction faction;

        // NOTE: detour/duplicate this to make this class work
        // RimWorld.Faction
        //public void TryOpenComms(Pawn negotiator)
        //{
        //    Find.WindowStack.Add(new Dialog_Negotiation(negotiator, this, FactionDialogMaker.FactionDialogFor(negotiator, this)));
        //}

        public static DiaNode FactionDialogFor(Pawn negotiator, Faction faction)
        {
            Diplomacy.negotiator = negotiator;
            Diplomacy.faction = faction;
            var text = faction.leader != null ? faction.leader.Name.ToStringFull : faction.name;
            if (faction.ColonyGoodwill < MinRelationsToCommunicate)
            {
                root = new DiaNode("FactionGreetingHostile".Translate(text));
            }
            else if (faction.ColonyGoodwill < MinRelationsFriendly)
            {
                var text2 = "FactionGreetingWary".Translate(text, negotiator.LabelBaseShort);
                text2 = text2.AdjustedFor(negotiator);
                root = new DiaNode(text2);
                root.options.Add(OfferGiftOption());
            }
            else
            {
                root = new DiaNode("FactionGreetingWarm".Translate(text, negotiator.LabelBaseShort));
                root.options.Add(OfferGiftOption());
                root.options.Add(RequestMilitaryAidOption());
            }
            var diaOption = new DiaOption("(" + "Disconnect".Translate() + ")") {resolveTree = true};
            root.options.Add(diaOption);
            return root;
        }

        public static DiaOption OfferGiftOption()
        {
            var num = (from t in TradeUtility.AllSellableThings
                       where t.def == ThingDefOf.Silver
                       select t).Sum(t => t.stackCount);
            if (num < GiftSilverAmount)
            {
                var diaOption = new DiaOption("OfferGift".Translate() + " (" + "NeedSilverLaunchable".Translate(GiftSilverAmount) + ")");
                diaOption.Disable("NotEnoughSilver".Translate());
                return diaOption;
            }
            var goodwillDelta = GiftSilverGoodwillChange * negotiator.GetStatValue(StatDefOf.GiftImpact);
            var diaOption2 = new DiaOption("OfferGift".Translate() + " (" + "SilverForGoodwill".Translate(GiftSilverAmount, goodwillDelta.ToString("#####0")) + ")");
            diaOption2.action = delegate
            {
                TradeUtility.SellThingsOfType(ThingDefOf.Silver, GiftSilverAmount, null);
                faction.AffectGoodwillWith(Faction.OfColony, goodwillDelta);
            };
            var text = "SilverGiftSent".Translate(faction.name, Mathf.RoundToInt(goodwillDelta));
            diaOption2.link = new DiaNode(text)
            {
                options =
                {
                    OKToRoot()
                }
            };
            return diaOption2;
        }

        public static DiaOption RequestMilitaryAidOption()
        {
            var diaOption = new DiaOption("RequestMilitaryAid".Translate(MilitaryAidRelsChange));
            if (Find.ListerPawns.PawnsHostileToColony.Any())
            {
                if (!Find.ListerPawns.PawnsHostileToColony.Any(p => p.Faction != null && p.Faction.HostileTo(faction)))
                {
                    var source = (from pa in Find.ListerPawns.PawnsHostileToColony
                                                   select pa.Faction into fa
                                                   where fa != null && !fa.HostileTo(faction)
                                                   select fa).Distinct();
                    var origin = new object[2];
                    origin[0] = faction.name;
                    origin[1] = GenText.ToCommaList(from fa in source
                                                     select fa.name);
                    var diaNode = new DiaNode("MilitaryAidConfirmMutualEnemy".Translate(origin));
                    var diaOption2 = new DiaOption("CallConfirm".Translate())
                    {
                        action = CallForAid,
                        link = FightersSent()
                    };
                    var diaOption3 = new DiaOption("CallCancel".Translate()) {linkLateBind = ResetToRoot()};
                    diaNode.options.Add(diaOption2);
                    diaNode.options.Add(diaOption3);
                    diaOption.link = diaNode;
                    return diaOption;
                }
            }
            diaOption.action = CallForAid;
            diaOption.link = FightersSent();
            return diaOption;
        }

        public static DiaNode FightersSent()
        {
            return new DiaNode("MilitaryAidSent".Translate(faction.name))
            {
                options =
                {
                    OKToRoot()
                }
            };
        }

        public static void CallForAid()
        {
            faction.AffectGoodwillWith(Faction.OfColony, MilitaryAidRelsChange);
            var incidentParms = new IncidentParms
            {
                faction = faction,
                points = Rand.Range(150, 400)
            };
            DefDatabase<IncidentDef>.GetNamed("RaidFriendly").Worker.TryExecute(incidentParms);
        }

        public static DiaOption OKToRoot()
        {
            return new DiaOption("OK".Translate())
            {
                linkLateBind = ResetToRoot()
            };
        }

        public static Func<DiaNode> ResetToRoot()
        {
            return () => FactionDialogFor(negotiator, faction);
        }
    }
}
