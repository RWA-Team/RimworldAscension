using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public static class SkyfallerUtil
    {
        public static void MakeDebrisCrashingAt(IntVec3 loc)
        {
            // Create a new falling drop pod
            var debrisCrashing = ThingMaker.MakeThing(ThingDef.Named("DebrisFlying"));
            // Spawn the falling drop pod
            GenSpawn.Spawn(debrisCrashing, loc);
        }

        public static void MakeDropPodCrashingAt(IntVec3 loc, DropPodInfo cargo)
        {
            // Create a new falling drop pod
            var dropPodCrashing = (DropPodFlying)ThingMaker.MakeThing(ThingDef.Named("DropPodFlying"));
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
                Log.Warning(string.Concat("DropThingsNear in RA failed to find a place to drop ", " near ", dropCenter, ". Dropping on random square instead."));
                // Try another way to get a drop spot
                CellFinderLoose.RandomCellWith(cell => cell.Walkable());
            }

            // Setup a new container for contents and config
            var cargo = new DropPodInfo();
            // Loop over things passed in params
            foreach (var group in thingsGroups)
            {
                // Foreach thing we find
                foreach (var thing in group)
                {
                    // Add it to the info container
                    cargo.containedThings.Add(thing);
                }
            }
            // Set the open delay on the info container
            cargo.openDelay = openDelay;

            // Call the main method to create the ship
            var wreck = (ShipWreckFlying)ThingMaker.MakeThing(ThingDef.Named("ShipWreckFlying"));
            // Set its content to what was passed in params
            wreck.cargo = cargo;
            // Spawn the falling ship part
            GenSpawn.Spawn(wreck, dropCenter);
        }

        public static void Impact(Thing skyfaller, Thing resultThing)
        {
            DoRoofPunch(skyfaller.Position);

            // max side length of drawSize or actual size etermine result crater radius
            var impactRadius = Mathf.Max(Mathf.Max(skyfaller.def.Size.x, skyfaller.def.Size.z), Mathf.Max(skyfaller.Graphic.drawSize.x, skyfaller.Graphic.drawSize.y)) * 2;

            // Throw some dust puffs
            for (var i = 0; i < 6; i++)
            {
                var loc = skyfaller.TrueCenter() + Gen.RandomHorizontalVector(1f);
                MoteThrower.ThrowDustPuff(loc, 1.2f);
            }

            // Throw a quick flash
            MoteThrower.ThrowLightningGlow(skyfaller.TrueCenter(), impactRadius);

            // Spawn the crater
            var crater = (Crater)ThingMaker.MakeThing(ThingDef.Named("Crater"));
            // adjust result crater size to the impact zone radius
            crater.impactRadius = impactRadius;
            // make explosion in the impact area
            DoImpactExplosion(skyfaller, impactRadius);

            // spawn the crater, rotated to the random angle, to provide visible variety
            GenSpawn.Spawn(crater, skyfaller.Position, Rot4.North);

            // place the impact result thing
            GenPlace.TryPlaceThing(resultThing, skyfaller.Position, ThingPlaceMode.Near);

            // MapComponent Injector
            if (!Find.Map.components.Exists(component => component.GetType() == typeof(MapComp_CameraShaker)))
                Find.Map.components.Add(new MapComp_CameraShaker());
            
            // Do a bit of camera shake for added effect
            MapComp_CameraShaker.DoShake(impactRadius * 0.025f);

            // Destroy incoming pod
            skyfaller.Destroy();
        }

        public static void DoImpactExplosion(Thing instigator, float radius)
        {
            var explosion = (Explosion)GenSpawn.Spawn(ThingDefOf.Explosion, instigator.Position);
            explosion.radius = radius;
            explosion.damType = DamageDefOf.Bomb;
            explosion.instigator = instigator;
            // damage is proportional to the size o the object
            explosion.damAmount = DamageDefOf.Bomb.explosionDamage*Mathf.RoundToInt(Mathf.Pow(radius, 2));
            explosion.source = instigator.def;
            explosion.applyDamageToExplosionCellsNeighbors = true;
            explosion.ExplosionStart(null);
        }

        // Punch the roof, if needed
        public static void DoRoofPunch (IntVec3 position)
        {
            var roof = position.GetRoof();
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
                    for (var j = 0; j < 3; j++)
                    {
                        FilthMaker.MakeFilth(position, roof.filthLeaving);
                    }
                }
            }
        }
    }
}
