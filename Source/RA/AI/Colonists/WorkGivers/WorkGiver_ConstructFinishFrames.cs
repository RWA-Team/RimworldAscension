using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_ConstructFinishFrames : WorkGiver_WorkWithTools
    {
        public WorkGiver_ConstructFinishFrames()
        {
            workType = "Construction";
        }

        public Job ActualJob(Thing target) => new Job(JobDefOf.FinishFrame, target);

        // search things throught designations is faster than searching designations through all things
        public static IEnumerable<Thing> AvailableTargets(Pawn pawn)
            => Find.ListerThings.AllThings.FindAll(target => target is Frame)
                .Where(target =>
                    target.Faction == pawn.Faction && GenConstruct.CanConstruct(target, pawn) &&
                    (target as Frame).MaterialsNeeded().Count == 0 &&
                    pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

        // keep tool if it could be used for other jobs
        public override bool ShouldKeepTool(Pawn pawn)
            => base.ShouldKeepTool(pawn) ||
               pawn.workSettings.WorkIsActive(WorkTypeDefOf.Repair) && WorkGiver_Repair.AvailableTargets(pawn).Any();

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
            => DoJobWithTool(pawn, AvailableTargets(pawn), ActualJob, ShouldKeepTool);
    }
}
