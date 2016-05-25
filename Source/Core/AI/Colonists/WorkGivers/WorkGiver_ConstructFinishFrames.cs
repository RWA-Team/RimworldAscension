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

        public Job ActualJob(Thing target)
        {
            // finish frame job allocation
            return new Job(JobDefOf.FinishFrame, target);
        }

        // search things throught designations is faster than searching designations through all things
        public static IEnumerable<Thing> AvailableTargets(Pawn pawn)
        {
            IEnumerable<Thing> designatedTargets = Find.ListerThings.AllThings.FindAll(target => target is Frame);

            return designatedTargets.Where(target => target.Faction == pawn.Faction && GenConstruct.CanConstruct(target, pawn) && (target as Frame).MaterialsNeeded().Count == 0 && pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            return DoJobWithTool(pawn, AvailableTargets(pawn), ActualJob, WorkGiver_Repair.AvailableTargets(pawn).Any());
        }
    }
}
