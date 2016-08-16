using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Repair : WorkGiver_WorkWithTools
    {
        public override string WorkType => "Construction";

        public override Job JobWithTool(TargetInfo target) => new Job(JobDefOf.Repair, target.Thing);

        public override List<TargetInfo> Targets(Pawn pawn)
            => AvailableTargets(pawn);

        // search things throught designations is faster than searching designations through all things
        public static List<TargetInfo> AvailableTargets(Pawn pawn)
            => ListerBuildingsRepairable.RepairableBuildings(pawn.Faction)
                .Where(target =>
                    target.Faction == pawn.Faction && Find.AreaHome[target.Position] &&
                    target.def.useHitPoints && target.HitPoints < target.MaxHitPoints &&
                    Find.DesignationManager.DesignationOn(target, DesignationDefOf.Deconstruct) == null &&
                    !target.IsBurning() &&
                    pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger())).Select(thing => new TargetInfo(thing)).ToList();
    }
}
