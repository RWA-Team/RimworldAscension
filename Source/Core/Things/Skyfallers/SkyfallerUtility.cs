using System.Collections.Generic;

using Verse;
using Verse.Sound;
using UnityEngine;
using RimWorld;

namespace RA
{
    public static class SkyfallerUtility
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

        public static void MakeShipWreckCrashingAt(IntVec3 dropCenter, List<List<Thing>> thingsGroups, int openDelay = 120, bool canInstaDropDuringInit = true, bool leaveSlag = false, bool canRoofPunch = false)
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
                intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable());
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

        public static void Impact(Thing skyfaller, Thing resultThing, float explosionDamageMultiplier)
        {
            DoRoofPunch(skyfaller.Position);

            // max side length of drawSize or actual size etermine result crater radius
            float impactRadius = Mathf.Max(Mathf.Max(skyfaller.def.Size.x, skyfaller.def.Size.z), Mathf.Max(skyfaller.Graphic.drawSize.x, skyfaller.Graphic.drawSize.y)) * 2;

            // Throw some dust puffs
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = skyfaller.TrueCenter() + Gen.RandomHorizontalVector(1f);
                MoteThrower.ThrowDustPuff(loc, 1.2f);
            }

            // Throw a quick flash
            MoteThrower.ThrowLightningGlow(skyfaller.TrueCenter(), impactRadius);

            // Spawn the crater
            Crater crater = (Crater)ThingMaker.MakeThing(ThingDef.Named("Crater"));
            // adjust result crater size to the impacr zone radius
            crater.impactRadius = impactRadius;
            // make explosion in the impact area
            GenExplosion.DoExplosion(skyfaller.Position, impactRadius, DamageDefOf.Bomb, skyfaller, null, null, explosionDamageMultiplier);

            // spawn the crater, rotated to the random angle, to provide visible variety
            GenSpawn.Spawn(crater, skyfaller.Position, Rot4.North);

            // Spawn the impact result thing
            GenSpawn.Spawn(resultThing, skyfaller.Position, skyfaller.Rotation);

            // MapComponent Injector
            if (!Find.Map.components.Exists(component => component.GetType() == typeof(MapCompCameraShaker)))
                Find.Map.components.Add(new MapCompCameraShaker());
            
            // Do a bit of camera shake for added effect
            MapCompCameraShaker.DoShake(impactRadius * 0.1f);

            // Destroy incoming pod
            skyfaller.Destroy();
        }

        // Punch the roof, if needed
        public static void DoRoofPunch (IntVec3 position)
        {
            RoofDef roof = position.GetRoof();
            // If there was actually a roof
            if (roof != null)
            {
                // If we can punch through
                if (!roof.soundPunchThrough.NullOrUndefined())
                {
                    // Play punch sound
                    roof.soundPunchThrough.PlayOneShot(position);
                }
                // If the roof def is to leave filth
                if (roof.filthLeaving != null)
                {
                    // Drop some filth
                    for (int j = 0; j < 3; j++)
                    {
                        FilthMaker.MakeFilth(position, roof.filthLeaving, 1);
                    }
                }
            }
        }
    }
}
