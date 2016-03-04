using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class SignalHorn : ThingWithComps, IUsable
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            Action action = () =>
            {
                var job = new Job(DefDatabase<JobDef>.GetNamed("UseSurvivalGuide"), this);
                pawn.drafter.TakeOrderedJob(job);
            };
            yield return new FloatMenuOption("Examine " + LabelCap, action);
        }

        public void UsedBy(Pawn pawn)
        {
            CallForHelp();
        }

        public void CallForHelp()
        {
            var helpTeamParams = IncidentParmsUtility.GenerateThreatPointsParams();
            helpTeamParams.forced = true;
            helpTeamParams.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;

            var queuedIncident = new QueuedIncident(IncidentDef.Named("RaidFriendly"), helpTeamParams)
            {
                occurTick = Find.TickManager.TicksGame + IncidentWorker_RaidFriendly.HelpDelay.RandomInRange
            };
            Find.Storyteller.incidentQueue.Add(queuedIncident);
        }
    }
}