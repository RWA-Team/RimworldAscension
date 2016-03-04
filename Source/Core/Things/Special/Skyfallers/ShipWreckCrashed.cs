using RimWorld;
using Verse;
using Verse.Sound;

namespace RA
{
    public class ShipWreckCrashed : DropPodCrashed
    {
        // used to spawn things after delay automatically
        public override void Deploy()
        {
            // Loop over all contents
            foreach (var thing in cargo.containedThings)
            {
                // Place thing in world
                GenPlace.TryPlaceThing(thing, new IntVec3(Position.x, Position.y, Position.z - 2), ThingPlaceMode.Near);
                // If its a pawn
                var pawn = thing as Pawn;
                // If its a humanlike pawn
                if (pawn != null && pawn.RaceProps.Humanlike)
                {
                    // Record a tale
                    TaleRecorder.RecordTale(TaleDef.Named("LandedInPod"), pawn);
                }
            }
            // All contents dealt with, clear list
            cargo.containedThings.Clear();
            // Play open sound
            OpenSound.PlayOneShot(Position);
        }

        // used to spawn things after scavenge action
        public override void SpawnScavengebles()
        {
            // Generate some steel slags
            for (var j = 0; j < 7; j++)
            {
                var thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
                GenPlace.TryPlaceThing(thing, Position, ThingPlaceMode.Near);
            }

            // Generate survival guide
            var thing2 = ThingMaker.MakeThing(ThingDef.Named("SurvivalGuide_Neolithic"));
            GenPlace.TryPlaceThing(thing2, Position, ThingPlaceMode.Near);

            // Generate some silver
            var thing3 = ThingMaker.MakeThing(ThingDefOf.Silver);
            thing3.stackCount = thing3.def.stackLimit;
            GenPlace.TryPlaceThing(thing3, Position, ThingPlaceMode.Near);

            // NOTE: throw message
        }
    }
}
