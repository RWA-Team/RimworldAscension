using Verse;

namespace RA
{
    public static class AutoUtil
    {
        public static void TryAutoDesignate(Designator designator, string designationDefName, TargetInfo target)
        {
            if (target.HasThing ? designator.CanDesignateThing(target.Thing).Accepted : designator.CanDesignateCell(target.Cell).Accepted)
            {
                var designation = new Designation(target, DefDatabase<DesignationDef>.GetNamed(designationDefName));
                Find.DesignationManager.AddDesignation(designation);
            }
        }
    }
}
