using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;
using Random = System.Random;

namespace RA
{
    public class DropPodLanded : Building
    {
        public static readonly SoundDef OpenSound = SoundDef.Named("DropPodOpen"); // Open sound

        public DropPodInfo cargo; // Drop pods contents and config
        public List<IntVec3> damagedCells = new List<IntVec3>(); // list of smoke and spark generators

        public override void SpawnSetup()
        {
            // Do base setup
            base.SpawnSetup();

            // probability for the cell to throw smoke motes, halved  with each iteration
            var chance = 1f;
            foreach (var cell in GenAdj.CellsOccupiedBy(this))
            {
                if (chance > new Random().NextDouble())
                    damagedCells.Add(cell);
                chance /= 2;
            }
        }

        public override void Tick()
        {
            foreach (var damagedCell in damagedCells)
            {
                // Throw smoke mote
                RA_Motes.ThrowSmokeBlack(damagedCell.ToVector3(), 0.5f);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (mode == DestroyMode.Deconstruct)
            {
                Deploy();
            }
        }

        // used to spawn things after delay automatically
        public void Deploy()
        {
            // Play open sound
            OpenSound.PlayOneShot(Position);
            // loop over all contents
            foreach (var thing in cargo.containedThings)
            {
                // Place thing in world
                GenPlace.TryPlaceThing(thing, Position, ThingPlaceMode.Near);

                // If its a pawn
                var pawn = thing as Pawn;
                // if its a humanlike pawn
                if (pawn != null && pawn.RaceProps.Humanlike)
                {
                    // Record a tale
                    TaleRecorder.RecordTale(TaleDef.Named("LandedInPod"), pawn);

                    // kill slavers
                    if (pawn.kindDef.defName == "SpaceSlaverDead")
                        HealthUtility.GiveInjuriesToKill(pawn);
                }
            }
            cargo.containedThings.Clear();
        }

        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();

            Scribe_Deep.LookDeep(ref cargo, "cargo");
            Scribe_Collections.LookList(ref damagedCells, "damagedCells", LookMode.Value);
        }
    }
}