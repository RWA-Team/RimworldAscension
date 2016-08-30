using RimWorld;
using Verse;

namespace RA
{
    public static class RA_DebugThingPlaceHelper
    {
        public static bool IsDebugSpawnable(ThingDef def)
        {
            return def.forceDebugSpawnable || (def.thingClass != typeof(Corpse) && !def.IsBlueprint && !def.IsFrame && def != ThingDefOf.DropPod && def.thingClass != typeof(MinifiedThing) && def.thingClass != typeof(UnfinishedThing) && !def.destroyOnDrop && (def.category == ThingCategory.Filth || def.category == ThingCategory.Item || def.category == ThingCategory.Plant || def.category == ThingCategory.Ethereal || (def.category == ThingCategory.Building && def.building.isNaturalRock) || (def.category == ThingCategory.Building && def.designationCategory.NullOrEmpty())));
        }
    }
}