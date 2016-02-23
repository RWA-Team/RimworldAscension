using System;
using Verse;
using System.Collections.Generic;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace RA
{
    public class ShipWreckCrashed : DropPodCrashed
    {
        // used to spawn things after delay automatically
        public override void Deploy()
        {
            // Loop over all contents
            for (int i = 0; i < this.cargo.containedThings.Count; i++)
            {
                // Setup a new thing
                Thing thing = this.cargo.containedThings[i];
                // Place thing in world
                GenPlace.TryPlaceThing(thing, new IntVec3(this.Position.x, this.Position.y, this.Position.z - 2), ThingPlaceMode.Near);
                // If its a pawn
                Pawn pawn = thing as Pawn;
                // If its a humanlike pawn
                if (pawn != null && pawn.RaceProps.Humanlike)
                {
                    // Record a tale
                    TaleRecorder.RecordTale(TaleDef.Named("LandedInPod"), new object[]{pawn});
                }
            }
            // All contents dealt with, clear list
            this.cargo.containedThings.Clear();
            // Play open sound
            ShipWreckCrashed.OpenSound.PlayOneShot(base.Position);
        }

        // used to spawn things after scavenge action
        public override void SpawnScavengebles()
        {
            // Generate some steel slags
            for (int j = 0; j < 7; j++)
            {
                Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
                GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near);
            }

            // Generate survival guide
            Thing thing2 = ThingMaker.MakeThing(ThingDef.Named("SurvivalGuide_Neolithic"));
            GenPlace.TryPlaceThing(thing2, base.Position, ThingPlaceMode.Near);

            // Generate some silver
            Thing thing3 = ThingMaker.MakeThing(ThingDefOf.Silver);
            thing3.stackCount = thing3.def.stackLimit;
            GenPlace.TryPlaceThing(thing3, base.Position, ThingPlaceMode.Near);

            // NOTE: throw message
        }
    }
}
