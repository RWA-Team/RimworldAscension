using RimWorld;
using Verse;

namespace RA
{
    public class RA_Designator_Mine : Designator_Mine
    {
        // make mine designator work after special research is made
        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            return Find.DesignationManager.DesignationAt(thing.Position, DesignationDefOf.Mine) == null &&
                   thing.def.mineable;
        }
    }
}