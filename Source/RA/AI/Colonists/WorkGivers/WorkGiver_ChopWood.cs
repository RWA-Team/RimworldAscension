using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_ChopWood : WorkGiver_WorkWithTools
    {
        public WorkGiver_ChopWood()
        {
            workType = "Woodchopping";
        }

        public Job ActualJob(Thing target)
            => (target as Plant).HarvestableNow ? new Job(JobDefOf.Harvest, target) : new Job(JobDefOf.CutPlant, target);

        // search things throught designations is faster than searching designations through all things
        public static IEnumerable<Thing> AvailableTargets(Pawn pawn)
            => Find.DesignationManager.allDesignations.FindAll(designation =>
                designation.def == DefDatabase<DesignationDef>.GetNamed("ChopWood"))
                .Select(designation => designation.target.Thing)
                .Where(target =>
                    !target.IsBurning() && pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
            => DoJobWithTool(pawn, AvailableTargets(pawn), ActualJob, ShouldKeepTool);
    }
}