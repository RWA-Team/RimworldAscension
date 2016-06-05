using RimWorld;
using Verse;

namespace RA
{
    public class RA_Designator_PlantsHarvest : Designator_PlantsHarvest
    {
        // make plant harvest designator work on plants with "Standart" harvest tag only
        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            if (Find.DesignationManager.DesignationOn(thing, designationDef) == null)
            {
                var plant = thing as Plant;
                if (plant?.def.plant != null && plant.HarvestableNow && plant.def.plant.harvestTag == "Standard")
                    return true;
                return "MessageMustDesignatePlants".Translate();
            }
            return false;
        }
    }
}
