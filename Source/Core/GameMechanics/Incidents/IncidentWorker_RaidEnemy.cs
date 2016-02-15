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
            float curPoints = parms.points;
            if (curPoints <= 0f)
            {
                curPoints = 999999f;
            }
            return (from faction in Find.FactionManager.AllFactions
                    where faction.HostileTo(Faction.OfColony) && faction.def.techLevel <= Faction.OfColony.def.techLevel && curPoints >= faction.def.MinPointsToGeneratePawnGroup() && (float)GenDate.DaysPassed >= faction.def.earliestRaidDays
                    select faction).TryRandomElementByWeight((Faction fac) => fac.def.raidCommonality, out parms.faction);
        }

        protected override void ResolveRaidStrategy(IncidentParms parms)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            parms.raidStrategy = (from d in DefDatabase<RaidStrategyDef>.AllDefs
                                  where d.Worker.CanUseWith(parms)
                                  select d).RandomElementByWeight((RaidStrategyDef d) => d.selectionChance);
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
                    str = "EnemyRaidWalkIn".Translate(new object[]
				{
					parms.faction.def.pawnsPlural,
					parms.faction.name
				});
                    break;
                case PawnsArriveMode.EdgeDrop:
                    str = "EnemyRaidEdgeDrop".Translate(new object[]
				{
					parms.faction.def.pawnsPlural,
					parms.faction.name
				});
                    break;
                case PawnsArriveMode.CenterDrop:
                    str = "EnemyRaidCenterDrop".Translate(new object[]
				{
					parms.faction.def.pawnsPlural,
					parms.faction.name
				});
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
