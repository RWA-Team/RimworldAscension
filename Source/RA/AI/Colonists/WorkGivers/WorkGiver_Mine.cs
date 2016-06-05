using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Mine : WorkGiver_WorkWithTools
    {
        public WorkGiver_Mine()
        {
            workType = "Mining";
        }

        public Job ActualJob(Thing target) => new Job(JobDefOf.Mine, target, 1500, true);

        // search things throught designations is faster than searching designations through all things
        public static IEnumerable<Thing> AvailableTargets(Pawn pawn)
            => Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Mine)
                .Select(designation => MineUtility.MineableInCell(designation.target.Cell))
                .Where(target => pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
            => DoJobWithTool(pawn, AvailableTargets(pawn), ActualJob, ShouldKeepTool);
    }
}
