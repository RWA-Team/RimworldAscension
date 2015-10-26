using System.Collections.Generic;
using Verse;
using RimWorld;

namespace RA
{
    public static class DropShipUtility
    {
        public static void MakeDropPodCrashingAt(IntVec3 loc, DropPodInfo cargo)
        {
            // Create a new falling drop pod
            DropPodCrashing dropPodCrashing = (DropPodCrashing)ThingMaker.MakeThing(ThingDef.Named("DropPodCrashing"), null);
            // Set its content to what was passed in params
            dropPodCrashing.cargo = cargo;
            // Spawn the falling drop pod
            GenSpawn.Spawn(dropPodCrashing, loc);
        }

        public static void MakeShipWreckCrashingAt(IntVec3 dropCenter, List<List<Thing>> thingsGroups, int openDelay = 120, bool canInstaDropDuringInit = true, bool leaveSlag = false, bool canRoofPunch = true)
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
            DropPodInfo cargo = new DropPodInfo();
            // Loop over things passed in params
            foreach (List<Thing> group in thingsGroups)
            {
                // Foreach thing we find
                foreach (Thing thing in group)
                {
                    // Add it to the info container
                    cargo.containedThings.Add(thing);
                }
            }
            // Set the open delay on the info container
            cargo.openDelay = openDelay;

            // Call the main method to create the ship
            ShipWreckCrashing wreck = (ShipWreckCrashing)ThingMaker.MakeThing(ThingDef.Named("ShipWreckCrashing"));
            // Set its content to what was passed in params
            wreck.cargo = cargo;
            // Spawn the falling ship part
            GenSpawn.Spawn(wreck, dropCenter);
        }
    }
}
