using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Hunt : WorkGiver_WorkWithTools
    {
        public Thing closestAvailableVein;

        public WorkGiver_Hunt()
        {
            workType = "Hunting";
        }

        public Job ActualJob(Thing target) => new Job(JobDefOf.Hunt, target);

        // search things throught designations is faster than searching designations through all things
        public static IEnumerable<Thing> AvailableTargets(Pawn pawn)
            => Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Hunt)
                .Select(designation => designation.target.Thing)
                .Where(target => pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

        // this orkgiver doesn't make pawn to return the tool (weapon)
        public override bool ShouldKeepTool(Pawn pawn) => true;

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
            => DoJobWithTool(pawn, AvailableTargets(pawn), ActualJob, ShouldKeepTool);
    }
}
