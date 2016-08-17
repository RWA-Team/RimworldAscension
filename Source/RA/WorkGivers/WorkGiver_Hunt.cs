using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Hunt : WorkGiver_WorkWithTools
    {
        public override string WorkType => "Hunting";

        public override Job JobWithTool(TargetInfo target) => new Job(JobDefOf.Hunt, target.Thing);

        public override List<TargetInfo> Targets(Pawn pawn)
            => AvailableTargets(pawn);

        // search things throught designations is faster than searching designations through all things
        public static List<TargetInfo> AvailableTargets(Pawn pawn)
            => Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Hunt)
                .Select(designation => designation.target)
                .Where(target => pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger())).ToList();
    }
}
