using System.Collections.Generic;
using Verse;
using RimWorld;

namespace RA
{
    public static class DropShipUtility
    {
        public static void MakeDropShipAt(IntVec3 loc, DropShipInfo info)
        {
            // Create a new falling ship part
            DropShipIncoming dropShipIncoming = (DropShipIncoming)ThingMaker.MakeThing(ThingDef.Named("DropShipIncoming"));
            // Set its content to what was passed in params
            dropShipIncoming.contents = info;
            // Spawn the falling ship part
            GenSpawn.Spawn(dropShipIncoming, loc);
        }

        public static void MakeDropPodCrashingAt(IntVec3 c, DropPodCrashingInfo info)
        {
            // Create a new falling drop pod
            DropPodCrashingIncoming dropPodCrashingIncoming = (DropPodCrashingIncoming)ThingMaker.MakeThing(ThingDef.Named("DropPodCrashingIncoming"), null);
            // Set its contents to what was passed in params
            dropPodCrashingIncoming.contents = info;
            // Spawn the falling drop pod
            GenSpawn.Spawn(dropPodCrashingIncoming, c);
        }

        public static void CreateDropShipAt(IntVec3 dropCenter, List<List<Thing>> thingsGroups, int openDelay = 300, bool canInstaDropDuringInit = true, bool leaveSlag = false, bool canRoofPunch = true)
        {
            // Set a var to store drop cell
            IntVec3 intVec;
            // If we cant find a dropspot
            if (!DropCellFinder.TryFindDropSpotNear(dropCenter, out intVec, true, canRoofPunch))
            {
                // Log an error
                Log.Warning(string.Concat(new object[]
                {
                        "DropThingsNear in Ascension failed to find a place to drop ",
                        " near ",
                        dropCenter,
                        ". Dropping on random square instead."
                }));
                // Try another way to get a drop spot
                intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(), 1000);
            }

            // Setup a new container for contents and config
            DropShipInfo dropShipInfo = new DropShipInfo();
            // Loop over things passed in params
            foreach (List<Thing> current in thingsGroups)
            {
                // Foreach thing we find
                foreach (Thing current3 in current)
                {
                    // Add it to the info container
                    dropShipInfo.containedThings.Add(current3);
                }
            }
            // Set the open delay on the info container
            dropShipInfo.openDelay = openDelay;
            // Call the main method to create the ship
            DropShipUtility.MakeDropShipAt(intVec, dropShipInfo);
        }
    }
}
