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

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override bool ShouldSkip(Pawn pawn)
        {
            return !Find.DesignationManager.DesignationsOfDef(designationDef).Any();
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            foreach (var designation in Find.DesignationManager.DesignationsOfDef(designationDef))
            {
                var targetCell = designation.target.Cell;
                if (targetCell.InBounds() && pawn.CanReserveAndReach(designation.target, PathEndMode.ClosestTouch, pawn.NormalMaxDanger()))
                {
                     yield return designation.target.Cell;
                }
            }
        }

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        {
            return Find.DesignationManager.DesignationAt(c, designationDef) != null && pawn.CanReserveAndReach(c, PathEndMode.ClosestTouch, pawn.NormalMaxDanger());
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 cell)
        {
            return new Job(jobDef, new TargetInfo(cell));
        }
    }
}