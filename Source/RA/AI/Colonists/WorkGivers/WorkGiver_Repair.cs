using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Repair : WorkGiver_WorkWithTools
    {
        public WorkGiver_Repair()
        {
            workType = "Construction";
        }

        public Job ActualJob(Thing target) => new Job(JobDefOf.Repair, target);

        // search things throught designations is faster than searching designations through all things
        public static IEnumerable<Thing> AvailableTargets(Pawn pawn)
            => ListerBuildingsRepairable.RepairableBuildings(pawn.Faction)
                .Where(target =>
                    target.Faction == pawn.Faction && Find.AreaHome[target.Position] &&
                    target.def.useHitPoints && target.HitPoints < target.MaxHitPoints &&
                    Find.DesignationManager.DesignationOn(target, DesignationDefOf.Deconstruct) == null &&
                    !target.IsBurning() &&
                    pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

        // keep tool if it could be used for other jobs
        public override bool ShouldKeepTool(Pawn pawn)
            => base.ShouldKeepTool(pawn) ||
               pawn.workSettings.WorkIsActive(WorkTypeDefOf.Construction) &&
               WorkGiver_ConstructFinishFrames.AvailableTargets(pawn).Any();

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
            => DoJobWithTool(pawn, AvailableTargets(pawn), ActualJob, ShouldKeepTool);
    }
}
