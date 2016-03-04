using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class IncidentWorker_RaidEnemy : IncidentWorker_Raid
    {
        public override bool TryExecute(IncidentParms parms)
        {
            if (base.TryExecute(parms))
            {
                Find.TickManager.slower.SignalForceNormalSpeedShort();
                Find.StoryWatcher.statsRecord.numRaidsEnemy++;
                return true;
            }
            return false;
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            if (parms.faction != null)
            {
                return true;
            }
            var curPoints = parms.points;
            if (curPoints <= 0f)
            {
                curPoints = 999999f;
            }
            return (from faction in Find.FactionManager.AllFactions
                    where faction.HostileTo(Faction.OfColony) && faction.def.techLevel <= Faction.OfColony.def.techLevel && curPoints >= faction.def.MinPointsToGeneratePawnGroup() && GenDate.DaysPassed >= faction.def.earliestRaidDays
                    select faction).TryRandomElementByWeight(fac => fac.def.raidCommonality, out parms.faction);
        }

        protected override void ResolveRaidStrategy(IncidentParms parms)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            parms.raidStrategy = (from def in DefDatabase<RaidStrategyDef>.AllDefs
                                  where def.Worker.CanUseWith(parms)
                                  select def).RandomElementByWeight(d => d.selectionChance);
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy;
        }

        protected override string GetLetterText(IncidentParms parms)
        {
            string str = null;
            switch (parms.raidArrivalMode)
            {
                case PawnsArriveMode.EdgeWalkIn:
                    str = "EnemyRaidWalkIn".Translate(parms.faction.def.pawnsPlural, parms.faction.name);
                    break;
                case PawnsArriveMode.EdgeDrop:
                    str = "EnemyRaidEdgeDrop".Translate(parms.faction.def.pawnsPlural, parms.faction.name);
                    break;
                case PawnsArriveMode.CenterDrop:
                    str = "EnemyRaidCenterDrop".Translate(parms.faction.def.pawnsPlural, parms.faction.name);
                    break;
            }
            str += "\n\n";
            return str + parms.raidStrategy.arrivalTextEnemy;
        }

        protected override LetterType GetLetterType()
        {
            return LetterType.BadUrgent;
        }
    }
}
