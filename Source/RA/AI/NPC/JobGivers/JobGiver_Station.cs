using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    class JobGiver_Station : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Wait)
            {
                return null;
            }
            return new Job(JobDefOf.Wait);
        }
    }
}
