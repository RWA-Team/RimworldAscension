using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Mine : WorkGiver_WorkWithTools
    {
        //used to keep current tool equipped if there are available unfinished jobs for this tool type
        public static bool hasPotentialJobs;
        
        public Thing closestAvailableVein;

        public WorkGiver_Mine()
        {
            workTypeName = "Mining";
        }

        public Job ActualJob(Thing target)
        {
            // new mining scheme without hauling rubble
            return new Job(JobDefOf.Mine, target, 1500, true);
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // search things throught designations is faster than searching designations through all things
            // all things marked for plantcutting or harvesting
            var designatedTargets = Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Mine).Select(designation => MineUtility.MineableInCell(designation.target.Cell));

            var availableTargets = designatedTargets.Where(target => pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

            hasPotentialJobs = availableTargets.Any();

            return DoJobWithTool(pawn, availableTargets, ActualJob);
        }
    }
}
