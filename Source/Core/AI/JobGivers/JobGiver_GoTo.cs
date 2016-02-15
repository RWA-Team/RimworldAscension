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
    public class JobGiver_GoTo : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            IntVec3 cell = pawn.mindState.duty.focus.Cell;
            if (!pawn.CanReach(cell, PathEndMode.OnCell, Danger.Some, false) || pawn.Position == cell)
            {
                return null;
            }

            return new Job(JobDefOf.Goto, cell)
            {
                locomotionUrgency = LocomotionUrgency.Walk,
            };
        }
    }
}
