using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_HunterHunt : WorkGiver_WorkWithTools
    {
        public Thing closestAvailableVein;

        public WorkGiver_HunterHunt()
        {
            workType = "Hunting";
        }

        public Job ActualJob(Thing target)
        {
            return new Job(JobDefOf.Hunt, target);
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // search things throught designations is faster than searching designations through all things
            var designatedTargets = Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Hunt).Select(designation => designation.target.Thing);

            var availableTargets = designatedTargets.Where(target => pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));
            
            return DoJobWithTool(pawn, availableTargets, ActualJob);
        }
    }
}
