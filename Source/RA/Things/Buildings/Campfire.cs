using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    class Campfire : RA_Building_WorkTable
    {
        public bool singalling = false;

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            // NOTE: required?
            foreach (var option in base.GetFloatMenuOptions(pawn))
                yield return option;

            yield return new FloatMenuOption("Start trading", () =>
            {
                var job = new Job(DefDatabase<JobDef>.GetNamed("MakeSignalSmoke"), this);
                pawn.drafter.TakeOrderedJob(job);
            });
        }

        //public override void ThrowSmoke(Vector3 loc, float size)
        //{
        //    RA_Motes.ThrowSmokeBlack_Signal(loc, size);
        //}

        //public void CallForMilitaryAid()
        //{
        //    var helpTeamParams = IncidentParmsUtility.GenerateThreatPointsParams();
        //    helpTeamParams.forced = true;
        //    helpTeamParams.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;

        //    var queuedIncident = new QueuedIncident(IncidentDef.Named("RaidFriendly"), helpTeamParams)
        //    {
        //        occurTick = Find.TickManager.TicksGame + IncidentWorker_RaidFriendly.HelpDelay.RandomInRange
        //    };
        //    Find.Storyteller.incidentQueue.Add(queuedIncident);
        //}
    }
}
