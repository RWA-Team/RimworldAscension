using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RA
{
    public static class RA_GenPlace
    {
        // allows to place items to container stacks
        public static bool TryPlaceDirect(Thing thing, IntVec3 loc, out Thing resultingThing,
            Action<Thing, int> placedAction = null)
        {
            #region CONTAINER CASE

            var container =
                Find.ThingGrid.ThingsListAt(loc).Find(building => building is Container && building.Spawned) as
                    Container;
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

#endregion

            #region VANILLA CASE

            // used to keep original thing reference
            var initialThing = thing;
            var flag = false;
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
                    var thing3 = thingList[i];
                    if (!thing3.CanStackWith(thing))
                    {
                        i++;
                    }
                    else
                    {
                        var stackCount = thing.stackCount;
                        if (thing3.TryAbsorbStack(thing, true))
                        {
                            resultingThing = thing3;
                            placedAction?.Invoke(thing3, stackCount);
                            return !flag;
                        }
                        resultingThing = null;
                        if (placedAction != null && stackCount != thing.stackCount)
                        {
                            placedAction(thing3, stackCount - thing.stackCount);
                        }
                        if (initialThing != thing)
                        {
                            initialThing.TryAbsorbStack(thing, false);
                        }
                        return false;
                    }
                }
            }
            resultingThing = GenSpawn.Spawn(thing, loc);
            placedAction?.Invoke(thing, thing.stackCount);
            var slotGroup = loc.GetSlotGroup();
            slotGroup?.parent?.Notify_ReceivedThing(resultingThing);
            return !flag;

            #endregion
        }
    }
}
