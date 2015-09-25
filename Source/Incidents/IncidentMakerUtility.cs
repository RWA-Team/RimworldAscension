using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RimworldAscension.Incidents
{
    public static class IncidentMakerUtility
    {
        private const float PointsPer1000Wealth = 12.5f;
        private const float PointsPerColonist = 43f;
        private const float WealthBuildingsModifier = 0.5f;
        private const float MinMaxSquadCost = 50f;
        private const float BuildingWealthFactor = 0.5f;
        private const float HalveLimitLo = 1000f;
        private const float HalveLimitHi = 2000f;

        public static IncidentParms DefaultParmsNow(StorytellerDef tellerDef, IncidentCategory incCat)
        {
            IncidentParms incidentParms = new IncidentParms();
            if (incCat == IncidentCategory.ThreatSmall || incCat == IncidentCategory.ThreatBig)
            {
                float wealthAmount = Find.StoryWatcher.watcherWealth.WealthItems + Find.StoryWatcher.watcherWealth.WealthBuildings * WealthBuildingsModifier;
                float wealthPoints = wealthAmount / 1000f * PointsPer1000Wealth;
                float colonistsPoints = (float)Find.ListerPawns.FreeColonistsCount * PointsPerColonist;
                incidentParms.points = wealthPoints + colonistsPoints;
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
                if (incidentParms.points > 1000f)
                {
                    if (incidentParms.points > 2000f)
                    {
                        incidentParms.points = 2000f + (incidentParms.points - 2000f) * 0.5f;
                    }
                    // Vanilla mistake? ("else" required)
                    incidentParms.points = 1000f + (incidentParms.points - 1000f) * 0.5f;
                }
            }
            return incidentParms;
        }
    }
}
