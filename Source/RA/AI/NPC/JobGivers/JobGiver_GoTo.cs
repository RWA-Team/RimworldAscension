using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobGiver_GoTo : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            var cell = pawn.mindState.duty.focus.Cell;
            if (pawn.Position == cell || !pawn.CanReach(cell, PathEndMode.OnCell, pawn.NormalMaxDanger()))
            {
                return null;
            }
            return new Job(JobDefOf.Goto, cell);
        }
    }
}