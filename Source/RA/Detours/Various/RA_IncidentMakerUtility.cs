using RimWorld;
using Verse;

namespace RA
{
    public static class RA_IncidentMakerUtility
    {
        public const float WealthBase = 2000f;
        public const float PointsPer1000Wealth = 11.5f;
        public const float PointsPerColonist = 40f;
        public const float BuildingWealthFactor = 0.5f;
        // soft cap
        public const float HalveLimitLo = 1000f;
        // hard cap
        public const float HalveLimitHi = 2000f;

        public static IncidentParms DefaultParmsNow(StorytellerDef tellerDef, IncidentCategory incCat)
        {
            var incidentParms = new IncidentParms();
            if (incCat == IncidentCategory.ThreatSmall || incCat == IncidentCategory.ThreatBig)
            {
                var wealthSummaryCost = Find.StoryWatcher.watcherWealth.WealthItems + Find.StoryWatcher.watcherWealth.WealthBuildings * BuildingWealthFactor;
                wealthSummaryCost -= WealthBase;
                if (wealthSummaryCost < 0f)
                {
                    wealthSummaryCost = 0f;
                }
                var threatPointsOfWealth = wealthSummaryCost / 1000f * PointsPer1000Wealth;
                var threatPointsOfPopulation = Find.MapPawns.FreeColonistsCount * PointsPerColonist;
                incidentParms.points = threatPointsOfWealth + threatPointsOfPopulation;
                incidentParms.points *= Find.StoryWatcher.watcherRampUp.TotalThreatPointsFactor;
                incidentParms.points *= Find.Storyteller.difficulty.threatScale;
                switch (Find.StoryWatcher.statsRecord.numThreatBigs)
                {
                    case 0:
                        incidentParms.points = 35f;
                        incidentParms.raidForceOneIncap = true;
                        incidentParms.raidNeverFleeIndividual = true;
                        break;
                    case 1:
                        incidentParms.points *= 0.5f;
                        break;
                    case 2:
                        incidentParms.points *= 0.7f;
                        break;
                    case 3:
                        incidentParms.points *= 0.8f;
                        break;
                    case 4:
                        incidentParms.points *= 0.9f;
                        break;
                    default:
                        incidentParms.points *= 1f;
                        break;
                }
                if (incidentParms.points < 0f)
                {
                    incidentParms.points = 0f;
                }
                if (incidentParms.points > HalveLimitLo)
                {
                    if (incidentParms.points > HalveLimitHi)
                    {
                        incidentParms.points = HalveLimitHi + (incidentParms.points - HalveLimitHi) * 0.5f;
                    }
                    incidentParms.points = HalveLimitLo + (incidentParms.points - HalveLimitLo) * 0.5f;
                }
            }
            return incidentParms;
        }
    }
}
