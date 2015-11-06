using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_ConstructFinishFrames : WorkGiver_WorkWithTools
    {
        //used to avoid conflict with Construction and Repair jobs cause they are using the same worktype tool
        public static bool currentWorkTypeKeepsTool;

        public WorkGiver_ConstructFinishFrames()
        {
            workTypeName = "Construction";
        }

        public Job ActualJob(Thing target)
        {
            // finish frame job allocation
            return new Job(JobDefOf.FinishFrame, target);
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // search things throught designations is faster than searching designations through all things
            // all things marked for plantcutting or harvesting
            IEnumerable<Thing> designatedTargets = Find.ListerThings.AllThings.FindAll(target => target is Frame);

            IEnumerable<Thing> availableTargets = designatedTargets.Where(target => target.Faction == pawn.Faction && GenConstruct.CanConstruct(target, pawn) && (target as Frame).MaterialsNeeded().Count == 0 && pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

            currentWorkTypeKeepsTool = availableTargets.Count() > 0 ? true : false;

            return DoJobWithTool(pawn, availableTargets, ActualJob, WorkGiver_Repair.currentWorkTypeKeepsTool);
        }
    }
}
