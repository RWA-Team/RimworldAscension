using RimWorld;
using Verse;

namespace RA
{
    public static class RA_GenPlace
    {
        // allows to place items to container stacks
        public static bool TryPlaceDirect(Thing thing, IntVec3 loc, out Thing resultingThing)
        {
            // container code part
            var container = Find.ThingGrid.ThingsListAt(loc).Find(building => building is Container && building.Spawned) as Container;
            if (container != null)
            {
                // stackables (like resources)
                if (thing.def.stackLimit > 1)
                {
                    foreach (var item in container.StoredItems)
                    {
                        // if item can stack with another
                        // Required, because thing reference is changed to the absorber, if absorbed
                        if (item.TryAbsorbStack(thing, true))
                        {
                            // former item gets absorbed and destroyed
                            resultingThing = item;
                            // hide texture and label of the stored item
                            container.HideItem(resultingThing);
                            return true;
                        }
                    }
                }

                resultingThing = GenSpawn.Spawn(thing, loc);
                // hide texture and label of the stored item
                container.HideItem(resultingThing);

                return true;
            }

            // boolean success indicator
            var flag = false;
            // vanilla code part
            if (thing.stackCount > thing.def.stackLimit)
            {
                thing = thing.SplitOff(thing.def.stackLimit);
                flag = true;
            }
            if (thing.def.stackLimit > 1)
            {
                var thingList = loc.GetThingList();
                var i = 0;
                while (i < thingList.Count)
                {
                    var thing2 = thingList[i];
                    if (!thing2.CanStackWith(thing))
                    {
                        i++;
                    }
                    else
                    {
                        // Required, because thing reference is changed to the absorber, if absorbed
                        if (thing2.TryAbsorbStack(thing, true))
                        {
                            resultingThing = thing2;
                            return !flag;
                        }
                        resultingThing = null;
                        return false;
                    }
                }
            }

            resultingThing = GenSpawn.Spawn(thing, loc);

            var slotGroup1 = loc.GetSlotGroup();
            slotGroup1?.parent?.Notify_ReceivedThing(resultingThing);
            return !flag;
        }
    }
}
