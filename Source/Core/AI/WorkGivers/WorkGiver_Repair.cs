using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Repair : WorkGiver_WorkWithTools
    {
        //used to keep current tool equipped if there are available unfinished jobs for this tool type
        public static bool hasPotentialJobs;

        public WorkGiver_Repair()
        {
            workTypeName = "Construction";
        }

        public Job ActualJob(Thing target)
        {
            // finish repairable job allocation
            return new Job(JobDefOf.Repair, target);
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // search things throught designations is faster than searching designations through all things
            // all things marked for plantcutting or harvesting
            IEnumerable<Thing> designatedTargets = ListerBuildingsRepairable.RepairableBuildings(pawn.Faction);

            IEnumerable<Thing> availableTargets = designatedTargets.Where(target => target.Faction == pawn.Faction && Find.AreaHome[target.Position] && pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()) && target.def.useHitPoints && target.HitPoints < target.MaxHitPoints && Find.DesignationManager.DesignationOn(target, DesignationDefOf.Deconstruct) == null && !target.IsBurning());

            hasPotentialJobs = availableTargets.Count() > 0 ? true : false;

            return DoJobWithTool(pawn, availableTargets, ActualJob);
        }
    }
}
