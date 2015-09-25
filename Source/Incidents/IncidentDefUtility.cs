using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RimworldAscension.Incidents
{
    // Copied from vanilla, bacause it is used by IncidentMaker_Classic
    public static class IncidentDefUtility
    {
        public static IEnumerable<IncidentDef> CurrentlyUsableIncidents
        {
            get
            {
                return from def in DefDatabase<IncidentDef>.AllDefs
                       where def.Worker.StorytellerCanUseNow()
                       select def;
            }
        }

        public static IncidentDef RandomThreatBig()
        {
            return (from def in DefDatabase<IncidentDef>.AllDefs
                    where def.category == IncidentCategory.ThreatBig
                    select def).RandomElementByWeight((IncidentDef def) => def.chance);
        }

        public static IncidentDef RandomThreatSmall()
        {
            return (from def in DefDatabase<IncidentDef>.AllDefs
                    where def.category == IncidentCategory.ThreatSmall
                    select def).RandomElementByWeight((IncidentDef def) => def.chance);
        }
    }
}
