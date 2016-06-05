using System.Linq;
using Verse;

namespace RA
{
    public class RA_StoreUtility
    {
        // added allowance to store more than 1 item in container cell
        public static bool NoStorageBlockersIn(IntVec3 targetCell, Thing haulable)
        {
            var list = Find.ThingGrid.ThingsListAt(targetCell);
            var container = list.Find(thing => thing.def.thingClass == typeof(Container) &&
                       thing.TryGetComp<CompContainer>() != null) as Container;
            if (container != null)
            {
                // allow if has free slots
                if (container.StoredItems.Count < container.TryGetComp<CompContainer>().Properties.itemsCap)
                {
                    return true;
                }
                // or can stack with other thing
                return container.StoredItems.Where(item => item.def == haulable.def)
                        .Any(item => item.stackCount < item.def.stackLimit);
            }
            foreach (var currentThing in list)
            {
                // no incopmlete stacks of the same thing
                if (currentThing.def.EverStoreable)
                {
                    if (currentThing.def != haulable.def)
                    {
                        return false;
                    }
                    if (currentThing.stackCount >= haulable.def.stackLimit)
                    {
                        return false;
                    }
                }
                // placed blueprint not standable
                if (currentThing.def.entityDefToBuild != null &&
                    currentThing.def.entityDefToBuild.passability != Traversability.Standable)
                {
                    return false;
                }
                // terrain not standable
                if (currentThing.def.surfaceType == SurfaceType.None &&
                    currentThing.def.passability != Traversability.Standable)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
