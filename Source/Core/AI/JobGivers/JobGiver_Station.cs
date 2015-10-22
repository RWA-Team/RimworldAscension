using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.SquadAI;

namespace RA
{
    class JobGiver_Station : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Wait)
            {
                Log.Message("stay");
                return null;
            }
            else
                return new Job(JobDefOf.Wait);
        }
    }
}
