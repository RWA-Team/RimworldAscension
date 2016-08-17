using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Mine : WorkGiver_WorkWithTools
    {
        public override string WorkType => "Mining";

        public override Job JobWithTool(TargetInfo target) => new Job(JobDefOf.Mine, target, 1500, true);

        public override List<TargetInfo> Targets(Pawn pawn)
            => AvailableTargets(pawn);

        // search things throught designations is faster than searching designations through all things
        public static List<TargetInfo> AvailableTargets(Pawn pawn)
            => Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Mine)
                .Select(designation => MineUtility.MineableInCell(designation.target.Cell))
                .Where(target => pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger())).Select(thing => new TargetInfo(thing)).ToList();
    }
}
