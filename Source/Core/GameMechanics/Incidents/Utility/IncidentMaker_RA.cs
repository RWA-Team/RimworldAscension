using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class IncidentMaker_RA : IncidentMaker
	{
        // number of days after start without any random incidents
        public static int graceDaysCount = 2;

        protected override float MinIncidentChancePopFactor => 0.05f;

        /* yield return doesn't break the sequence
         * called every 7500 ticks (QueueInterval), 4 times per game day
         * set is make anew due to changed game parameters
         */
        protected override IEnumerable<QueuedIncident> NewIncidentSet()
        {
            // Fixed incidents
            {
                //Day 3: visitors
                if (DaysPassed == 3)
                {
                    var qi = new QueuedIncident(IncidentDef.Named("VisitorGroup"));
                    yield return qi;
                }
            }

            // All random events start to happen after grace period
            if (DaysPassed > graceDaysCount)
            {
                // Disease incident
                foreach (var di in DiseaseIncidentMaker.DiseaseIncidentSet())
                {
                    yield return di;
                }

                // General incident
                if (Rand.MTBEventOccurs(Def.classic_RandomEventMTBDays, GenDate.TicksPerDay, QueueInterval))
                    yield return RandomIncidentOfCategory(IncidentCategory.Misc);

                // Small threats
                if (DaysPassed > 8 && Rand.MTBEventOccurs(Def.classic_ThreatSmallMTBDays, GenDate.TicksPerDay, QueueInterval))
                    yield return RandomIncidentOfCategory(IncidentCategory.ThreatSmall);

                // Big threats
                var daysSinceThreatBig = (float)(Find.TickManager.TicksGame - StoryState.LastThreatBigQueueTick) / GenDate.TicksPerDay;
                if (daysSinceThreatBig > Def.minDaysBetweenThreatBigs)
                {
                    if (Rand.MTBEventOccurs(Def.classic_ThreatBigMTBDays, GenDate.TicksPerDay, QueueInterval))
                        yield return RandomIncidentOfCategory(IncidentCategory.ThreatBig);
                }
            }
        }

        public QueuedIncident RandomIncidentOfCategory(IncidentCategory category)
        {
            // random weight determine element probalility compare to other elements
            var incidentDef = DefDatabase<IncidentDef>.AllDefs.Where(def => def.category == category).RandomElementByWeight(def => IncidentChanceAdjustedForPopulation(def));
            return new QueuedIncident(incidentDef, ParmsNow(incidentDef.category));
        }

        public override IncidentParms ParmsNow(IncidentCategory incCat)
        {
            return IncidentParmsUtility.GenerateThreatPointsParams();
        }

        // used for debug
		public void ForceQueueThreatBig()
		{
            IncQueue.Add(RandomIncidentOfCategory(IncidentCategory.ThreatBig));
		}
	}
}
