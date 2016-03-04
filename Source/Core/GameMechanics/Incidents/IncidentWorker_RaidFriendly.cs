using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class IncidentWorker_RaidFriendly : IncidentWorker_Raid
    {
        public static readonly IntRange HelpDelay = new IntRange(GenDate.TicksPerHour * 1, GenDate.TicksPerHour * 3);

        protected override bool StorytellerCanUseNowSub()
        {
            return (from p in Find.ListerPawns.PawnsHostileToColony
                    where !p.Downed
                    select p).Sum((Pawn p) => p.kindDef.combatPower) > 120f;
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            // skip if special faction is forced
            if (parms.faction != null)
            {
                return true;
            }

            var attackingFactions = (from pawn in Find.ListerPawns.PawnsHostileToColony
                                                      select pawn.Faction).Distinct();

            // make list of friendly factions with descending order by their goodwill
            var friendlyFactions = Find.FactionManager.AllFactionsVisible.Where(fac => fac != Faction.OfColony && fac.GoodwillWith(Faction.OfColony) >= 0f).ToList().OrderByDescending(fac => fac.ColonyGoodwill).ToList();

            foreach (var faction in friendlyFactions)
            {
                // goodwill >= 50
                if (faction.GoodwillWith(Faction.OfColony) >= 50f && faction.GoodwillWith(Faction.OfColony) < Rand.RangeInclusive(0, 100))
                {
                    parms.faction = faction;
                    return true;
                }
                // goodwill < 50, but hostile to any of attackers
                if (attackingFactions.Any(enemyFac => enemyFac.HostileTo(faction)) && faction.GoodwillWith(Faction.OfColony) < Rand.RangeInclusive(0, 100))
                {
                    parms.faction = faction;
                    return true;
                }
            }

            return false;
        }

        protected override void ResolveRaidStrategy(IncidentParms parms)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelFriendly;
        }

        protected override string GetLetterText(IncidentParms parms)
        {
            string str = null;
            switch (parms.raidArrivalMode)
            {
                case PawnsArriveMode.EdgeWalkIn:
                    str = "FriendlyRaidWalkIn".Translate(parms.faction.def.pawnsPlural, parms.faction.name);
                    break;
                case PawnsArriveMode.EdgeDrop:
                    str = "FriendlyRaidEdgeDrop".Translate(parms.faction.def.pawnsPlural, parms.faction.name);
                    break;
                case PawnsArriveMode.CenterDrop:
                    str = "FriendlyRaidCenterDrop".Translate(parms.faction.def.pawnsPlural, parms.faction.name);
                    break;
            }
            str += "\n\n";
            return str + parms.raidStrategy.arrivalTextFriendly;
        }

        protected override LetterType GetLetterType()
        {
            return LetterType.BadNonUrgent;
        }
    }
}