using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class Designator_ChopWood : Designator_Plants
    {
        public Designator_ChopWood()
        {
            defaultLabel = "DesignatorHarvestWood".Translate();
            defaultDesc = "DesignatorHarvestWoodDesc".Translate();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                icon = ContentFinder<Texture2D>.Get("UI/Designators/HarvestWood");
            });
            soundDragSustain = SoundDefOf.DesignateDragStandard;
            soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            useMouseIcon = true;
            soundSucceeded = SoundDefOf.DesignateHarvest;
            hotKey = KeyBindingDefOf.Misc1;
            designationDef = DefDatabase<DesignationDef>.GetNamed("ChopWood");
        }

        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            if (Find.DesignationManager.DesignationOn(thing, designationDef) == null)
            {
                var plant = thing as Plant;
                if (plant?.def.plant != null && plant.HarvestableNow && plant.def.plant.harvestTag == "Wood")
                    return true;
                return "MessageMustDesignateHarvestableWood".Translate();
            }
            return false;
        }
    }
}
