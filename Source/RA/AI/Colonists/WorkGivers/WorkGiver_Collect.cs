using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public abstract class WorkGiver_Collect : WorkGiver_Scanner
    {
        public DesignationDef designationDef;
        public JobDef jobDef;

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool ShouldSkip(Pawn pawn)
        {
            return !Find.DesignationManager.DesignationsOfDef(designationDef).Any();
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            // TODO: check if i need more detailed version
            return Find.DesignationManager.DesignationsOfDef(designationDef).Select(designation => designation.target.Cell);
        }

        // checks if job is still doable (previousPawnWeapons and reachability usually checked here too)
        public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        {
            return Find.DesignationManager.DesignationAt(c, designationDef) != null && pawn.CanReserveAndReach(c, PathEndMode.Touch, pawn.NormalMaxDanger());
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 cell)
        {
            return new Job(jobDef, new TargetInfo(cell));
        }
    }
}