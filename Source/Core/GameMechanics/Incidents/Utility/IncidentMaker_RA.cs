using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RA
{
	public class IncidentMaker_RA : IncidentMaker
	{
        // number of days after start without any random incidents
        public static int graceDaysCount = 2;

        protected override float MinIncidentChancePopFactor
		{
			get
			{
				return 0.05f;
			}
		}

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
                    QueuedIncident qi = new QueuedIncident(IncidentDef.Named("VisitorGroup"));
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
                if (Rand.MTBEventOccurs(Def.classic_RandomEventMTBDays, GenDate.TicksPerDay, IncidentMaker.QueueInterval))
                    yield return RandomIncidentOfCategory(IncidentCategory.Misc);

                // Small threats
                if (DaysPassed > 8 && Rand.MTBEventOccurs(Def.classic_ThreatSmallMTBDays, GenDate.TicksPerDay, IncidentMaker.QueueInterval))
                    yield return RandomIncidentOfCategory(IncidentCategory.ThreatSmall);

                // Big threats
                float daysSinceThreatBig = (float)(Find.TickManager.TicksGame - StoryState.LastThreatBigQueueTick) / GenDate.TicksPerDay;
                if (daysSinceThreatBig > Def.minDaysBetweenThreatBigs)
                {
                    if (Rand.MTBEventOccurs(Def.classic_ThreatBigMTBDays, GenDate.TicksPerDay, IncidentMaker.QueueInterval))
                        yield return RandomIncidentOfCategory(IncidentCategory.ThreatBig);
                }
            }
        }

        public QueuedIncident RandomIncidentOfCategory(IncidentCategory category)
        {
            // random weight determine element probalility compare to other elements
            IncidentDef incidentDef = DefDatabase<IncidentDef>.AllDefs.Where(def => def.category == category).RandomElementByWeight(def => IncidentChanceAdjustedForPopulation(def));
            return new QueuedIncident(incidentDef, this.ParmsNow(incidentDef.category));
        }

        public override IncidentParms ParmsNow(IncidentCategory incCat)
        {
            return IncidentParmsUtility.GenerateThreatPointsParams();
        }

        // used for debug
		public void ForceQueueThreatBig()
		{
            base.IncQueue.Add(RandomIncidentOfCategory(IncidentCategory.ThreatBig));
		}
	}
}
