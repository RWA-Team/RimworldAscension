using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_IncidentWorker_RaidEnemy : IncidentWorker_RaidEnemy
    {
        // added tech level check for raid faction (not higher than current)
        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            if (parms.faction != null)
            {
                return true;
            }
            var maxPoints = parms.points;
            if (maxPoints <= 0f)
            {
                maxPoints = 999999f;
            }
            return (from faction in Find.FactionManager.AllFactions
                    where faction.HostileTo(Faction.OfColony) && faction.def.techLevel <= Faction.OfColony.def.techLevel && maxPoints >= faction.def.MinPointsToGenerateNormalPawnGroup() && GenDate.DaysPassed >= faction.def.earliestRaidDays
                    select faction).TryRandomElementByWeight(fac => fac.def.raidCommonality, out parms.faction);
        }
    }
}