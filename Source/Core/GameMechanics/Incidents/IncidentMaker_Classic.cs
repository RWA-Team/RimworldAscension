using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RimworldAscension
{
	public class IncidentMaker_Classic : IncidentMaker
	{
        protected override float MinIncidentChancePopFactor
		{
			get
			{
				return 0.05f;
			}
		}
        protected override IEnumerable<QueuedIncident> NewIncidentSet()
        {
            //Special disease incidents
            foreach (var di in DiseaseIncidentMaker.DiseaseIncidentSet())
            {
                yield return di;
            }

            // Fixed early incidents
            {
                //Day 3: visitors
                if (IntervalsPassed == (3 * IntervalsPerDay) + 2)
                {
                    QueuedIncident qi = new QueuedIncident(IncidentDef.Named("VisitorGroup"));
                    qi.parms.points = Rand.Range(40, 100);
                    yield return qi;
                }

                //Day 4: Small threat
                if (IntervalsPassed == (4 * IntervalsPerDay) + 2)
                {
                    var qi = QueuedThreatSmall();
                    if (qi != null)
                        yield return qi;
                }

                //Day 5: Joiner
                if (IntervalsPassed == (5 * IntervalsPerDay) + 2)
                {
                    QueuedIncident qi = new QueuedIncident(IncidentDef.Named("WandererJoin"));
                    yield return qi;
                }

                //Day 9: First raid
                //9 days is over half of the 17-day threat cycle
                if (IntervalsPassed == (9 * IntervalsPerDay) + 2)
                {
                    var qi = QueuedThreatBig();
                    if (qi != null)
                        yield return qi;
                }
            }

            //Grace period for random events
            if (GenDate.DaysPassed <= 2)
                yield break;

            // Possibly yield a random general incident
            if (Rand.MTBEventOccurs(Def.classic_RandomEventMTBDays, GenDate.TicksPerDay, IncidentMaker.QueueInterval))
            {
                IncidentDef selectedDef = IncidentsOfCategory(IncidentCategory.Misc)
                                    .RandomElementByWeight(incDef => IncidentChanceAdjustedForPopulation(incDef));

                yield return new QueuedIncident(selectedDef);
            }

            // Yield threats (only during the last half of every threat cycle)
            int threatDays = DaysPassed - 2;
            float threatCyclePos = ((float)(threatDays % Def.threatCycleLength)) / (float)Def.threatCycleLength;
            if (threatDays > 0 && threatCyclePos > 0.5f)
            {
                //Big threats
                int ticksSinceThreatBig = Find.TickManager.TicksGame - StoryState.LastThreatBigQueueTick;
                if (ticksSinceThreatBig > Def.minDaysBetweenThreatBigs * GenDate.TicksPerDay)
                {
                    //You MUST have a big threat if you haven't had one for a whole half threat cycle, and you're near the
                    //end of the threat cycle
                    bool mustThreat = ticksSinceThreatBig > Def.threatCycleLength * 0.8 && threatCyclePos > 0.85f;
                    if (mustThreat || Rand.MTBEventOccurs(Def.classic_ThreatBigMTBDays, GenDate.TicksPerDay, IncidentMaker.QueueInterval))
                    {
                        var bt = QueuedThreatBig();
                        if (bt != null)
                            yield return bt;
                    }
                }

                //Small threats
                if (DaysPassed > 8 && Rand.MTBEventOccurs(Def.classic_ThreatSmallMTBDays, GenDate.TicksPerDay, IncidentMaker.QueueInterval))
                {
                    var st = QueuedThreatSmall();
                    if (st != null)
                        yield return st;
                }
            }
        } 

		public override IncidentParms ParmsNow(IncidentCategory incCat)
		{
			return IncidentMakerUtility.DefaultParmsNow(base.Def, incCat);
		}

		public QueuedIncident QueuedThreatBig()
		{
			IncidentDef incidentDef;
			if (Find.StoryWatcher.statsRecord.numRaidsEnemy <= 4)
			{
				incidentDef = DefDatabase<IncidentDef>.GetNamed("RaidEnemy", true);
			}
			else
			{
				incidentDef = (from def in DefDatabase<IncidentDef>.AllDefs
				where def.category == IncidentCategory.ThreatBig
				select def).RandomElementByWeight((IncidentDef def) => def.chance);
			}
			return new QueuedIncident(incidentDef, null)
			{
				parms = this.ParmsNow(incidentDef.category)
			};
		}

		public QueuedIncident QueuedThreatSmall()
		{
			IncidentDef incidentDef = IncidentDefUtility.RandomThreatSmall();
			return new QueuedIncident(incidentDef, null)
			{
				parms = this.ParmsNow(incidentDef.category)
			};
		}

		public void ForceQueueThreatBig()
		{
			base.IncQueue.Add(this.QueuedThreatBig());
		}
	}
}
