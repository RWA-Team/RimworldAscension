using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_ConstructFinishFrames : WorkGiver_WorkWithTools
    {
        public override string WorkType => "Construction";

        public override Job JobWithTool(TargetInfo target) => new Job(JobDefOf.FinishFrame, target.Thing);

        public override List<TargetInfo> Targets(Pawn pawn) => AvailableTargets(pawn);

        // search things throught designations is faster than searching designations through all things
        public static List<TargetInfo> AvailableTargets(Pawn pawn)
        {
            return Find.ListerThings.AllThings.FindAll(thing => thing is Frame)
                .Where(thing => thing.Faction == pawn.Faction && GenConstruct.CanConstruct(thing, pawn) &&
                                (thing as Frame).MaterialsNeeded().Count == 0 &&
                                pawn.CanReserveAndReach(thing, PathEndMode.Touch, pawn.NormalMaxDanger())).Select(thing => new TargetInfo(thing)).ToList();
        }
    }
}
