using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RA
{
    public static class IncidentParmsUtility
    {
        public const float WealthToPoints_Modifier = 12.5f;
        public const float PointsPerColonist = 50f;
        public const float WealthPerBuilding_Modifier = 0.5f;

        // used to decrease points influence above cirtain amount
        public static float SoftPointsCap = 1000f;
        // it will require 3000 points to reach 2000 HardPointsCap due to exceed halving
        public static float HardPointsCap = 2000f;

        public static IncidentParms GenerateThreatPointsParams()
        {
            IncidentParms incidentParms = new IncidentParms();

            // WealthItems is summary market value of all non-fogged haulables on map and colonists equipment
            float wealthAmount = Find.StoryWatcher.watcherWealth.WealthItems + Find.StoryWatcher.watcherWealth.WealthBuildings * WealthPerBuilding_Modifier;
            float wealthPoints = wealthAmount * WealthToPoints_Modifier / 1000f;
            float colonistsPoints = (float)Find.ListerPawns.FreeColonistsCount * PointsPerColonist;
            incidentParms.points = wealthPoints + colonistsPoints;
            incidentParms.points *= Find.StoryWatcher.watcherRampUp.TotalThreatPointsFactor;
            incidentParms.points *= Find.Storyteller.difficulty.threatScale;

            // NOTE: try to avoid that at all
            // vanilla hack to decrease threat points amount early on, based on the quantity of happened Big Threats
            switch (Find.StoryWatcher.statsRecord.numThreatBigs)
            {
                case 0:
                    incidentParms.points *= 0.4f;
                    break;
                case 1:
                    incidentParms.points *= 0.7f;
                    break;
                default:
                    incidentParms.points *= 1f;
                    break;
            }

            // halves the threat points amount which exceed the cap levels for each cap
            if (incidentParms.points > SoftPointsCap)
            {
                incidentParms.points = SoftPointsCap + (incidentParms.points - SoftPointsCap) * 0.5f;

                if (incidentParms.points > HardPointsCap)
                    incidentParms.points = HardPointsCap + (incidentParms.points - HardPointsCap) * 0.5f;
            }

            return incidentParms;
        }
    }
}
