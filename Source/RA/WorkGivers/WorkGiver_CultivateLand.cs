using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_CultivateLand : WorkGiver_WorkWithTools
    {
        public override string WorkType => "Digging";

        public override Job JobWithTool(TargetInfo target) => new Job(DefDatabase<JobDef>.GetNamedSilentFail("CultivateLand"), target.Cell);

        public override List<TargetInfo> Targets(Pawn pawn)
            => AvailableTargets(pawn);

        // search things throught designations is faster than searching designations through all things
        public static List<TargetInfo> AvailableTargets(Pawn pawn)
            => Find.DesignationManager.DesignationsOfDef(DefDatabase<DesignationDef>.GetNamedSilentFail("CultivateLand"))
                .Select(designation => designation.target)
                .Where(target => pawn.CanReserveAndReach(target, PathEndMode.ClosestTouch, pawn.NormalMaxDanger())).ToList();
    }
}
