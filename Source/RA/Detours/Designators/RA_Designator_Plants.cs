using RimWorld;
using Verse;

namespace RA
{
	public class RA_Designator_Plants : Designator_Plants
	{
        // make plant cut designator work on plants with no harvest tags only
	    public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            if (Find.DesignationManager.DesignationOn(thing, designationDef) == null)
            {
                var plant = thing as Plant;
                if (plant?.def.plant != null && plant.HarvestableNow && plant.def.plant.harvestTag == default(string))
                    return true;
                return "MessageMustDesignatePlants".Translate();
            }
            return false;
	    }
	}
}
