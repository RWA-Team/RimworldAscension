using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_ConstructFinishFrames : WorkGiver_WorkWithTools
    {
        //used to keep current tool equipped if there are available unfinished jobs for this tool type
        public static bool hasPotentialJobs;

        public WorkGiver_ConstructFinishFrames()
        {
            workTypeName = "Construction";
        }

        public Job ActualJob(Thing target)
        {
            // finish frame job allocation
            return new Job(JobDefOf.FinishFrame, target);
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // search things throught designations is faster than searching designations through all things
            // all things marked for plantcutting or harvesting
            IEnumerable<Thing> designatedTargets = Find.ListerThings.AllThings.FindAll(target => target is Frame);

            var availableTargets = designatedTargets.Where(target => target.Faction == pawn.Faction && GenConstruct.CanConstruct(target, pawn) && (target as Frame).MaterialsNeeded().Count == 0 && pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

            hasPotentialJobs = availableTargets.Any();

            return DoJobWithTool(pawn, availableTargets, ActualJob);
        }
    }
}
