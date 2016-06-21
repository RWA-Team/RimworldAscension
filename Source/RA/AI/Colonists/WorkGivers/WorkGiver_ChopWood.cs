using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_ChopWood : WorkGiver_WorkWithTools
    {
        public override string WorkType => "Woodchopping";

        public override Job JobWithTool(TargetInfo target)
            => (target.Thing as Plant).HarvestableNow
                ? new Job(JobDefOf.Harvest, target.Thing)
                : new Job(JobDefOf.CutPlant, target.Thing);

        public override List<TargetInfo> Targets(Pawn pawn) => AvailableTargets(pawn);

        // search things throught designations is faster than searching designations through all things
        public static List<TargetInfo> AvailableTargets(Pawn pawn)
        {
            return Find.DesignationManager.allDesignations.FindAll(designation =>
                designation.def == DefDatabase<DesignationDef>.GetNamed("ChopWood"))
                .Select(designation => designation.target)
                .Where(target => !target.IsBurning() &&
                                 pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger())).ToList();
        }
    }
}