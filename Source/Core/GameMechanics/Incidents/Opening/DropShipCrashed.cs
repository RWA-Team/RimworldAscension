using System;
using Verse;
using System.Collections.Generic;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace RA
{
    public class DropShipCrashed : Thing
    {
        public int age; // Life of thing in ticks
        public DropShipInfo info; // DropShip contents and config
        private static readonly SoundDef OpenSound = SoundDef.Named("DropPodOpen"); // Open sound
        public IEnumerable<IntVec3> occupiedCells = null; // List of cells the drop ship occupies
        IntVec3 smoker; // A random cell in the occupied list
        IntVec3 sparker; // A random cell in the occupied list
        public int sparkChance = 60; // // Chance for sparking effect to fire
        System.Random rand = new System.Random(); // Global random
        Effecter sparks = null; // Global effects variable
        public bool opened = false; // Whether the ship has opened or not

        public override void SpawnSetup()
        {
            // Do base setup
            base.SpawnSetup();
            // Setup list of cells the drop ship occupies
            occupiedCells = GenAdj.CellsOccupiedBy(this);
            // Pick a random cell for mote usage
            smoker = occupiedCells.RandomElement<IntVec3>();
            // And another
            sparker = occupiedCells.RandomElement<IntVec3>();
        }
        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();
            // Save age to save file
            Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
            // Save the contents and config to save file
            Scribe_Deep.LookDeep<DropShipInfo>(ref this.info, "info", new object[0]);
        }
        public override void Tick()
        {
            // If we havent opened yet
            if (opened == false)
            {
                // Increment age     
                this.age++;
                // If the age is more than the open delay
                if (this.age > this.info.openDelay)
                {
                    // Open the ship
                    this.ShipOpen();
                    opened = true;
                }
            }
            else
            {
                // Throw black smoke
                ThrowSmokeBlack(smoker.ToVector3Shifted(), 1f);
                // 
                // If we have no spark effect playing
                if (sparks == null)
                {
                    // Setup a new spark effect
                    sparks = new Effecter(DefDatabase<EffecterDef>.GetNamed("ElectricShort"));
                    // Trigger effect
                    sparks.Trigger(this, this);
                }
                // Randomise the effect a litte by stopping it every now and then
                if (Find.TickManager.TicksGame % sparkChance == UnityEngine.Random.Range(1, 20))
                {
                    int chance = rand.Next(0, 120);
                    // Stop the effect
                    sparks.Cleanup();
                    // Set variable to null so it refires next tick
                    sparks = null;
                }
                else
                {
                    // Continue effect
                    sparks.EffectTick(Position, this);
                }
            }
        }
        public static void ThrowSmokeBlack(Vector3 loc, float size)
        {
            // Only throw smoke every 10 ticks
            if (Find.TickManager.TicksGame % 10 != 0)
            {
                return;
            }
            // Make the mote
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_SmokeBlack"), null);
            // Set a size
            moteThrown.ScaleUniform = Rand.Range(1.7f, 2.7f) * size;
            // Set a rotation
            moteThrown.exactRotationRate = Rand.Range(-0.5f, 0.5f);
            // Set a position
            moteThrown.exactPosition = loc;
            // Set angle and speed
            moteThrown.SetVelocityAngleSpeed((float)Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
            // Spawn mote
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Call base destory method
            base.Destroy(mode);
            // Check destroy mod passed
            if (mode == DestroyMode.Deconstruct || mode == DestroyMode.Kill)
            {
                // iterate a few times
                for (int j = 0; j < 2; j++)
                {
                    // Drop some steel slag
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near);
                }
            }
        }

        private void ShipOpen()
        {
            // Loop over all contents
            for (int i = 0; i < this.info.containedThings.Count; i++)
            {
                // Setup a new thing
                Thing thing = this.info.containedThings[i];
                // Place thing in world
                GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near);
                // If its a pawn
                Pawn pawn = thing as Pawn;
                // If its a humanlike pawn
                if (pawn != null && pawn.RaceProps.Humanlike)
                {
                    // Record a tale
                    TaleRecorder.RecordTale(TaleDef.Named("LandedInPod"), new object[]
                    {
                        pawn
                    });
                }
            }
            // All contents dealt with, clear list
            this.info.containedThings.Clear();
            // Play open sound
            DropShipCrashed.OpenSound.PlayOneShot(base.Position);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            // Do base gizmos if required
            foreach (Gizmo curGizmo in base.GetGizmos())
            {
                yield return curGizmo;
            }

            // Setup a new command
            var comActScav = new Command_Action();
            if (comActScav != null)
            {
                // Command icon
                comActScav.icon = ContentFinder<Texture2D>.Get("UI/Gizmo/Scavenge");
                // Command label
                comActScav.defaultLabel = "Scavenge";
                // Command description
                comActScav.defaultDesc = "Find tools and materials within this building";
                // Command sound when activated
                comActScav.activateSound = SoundDef.Named("Click");
                // Action to call
                comActScav.action = new Action(ScavengeBuilding);
                // Add new command
                if (comActScav.action != null)
                {
                    yield return comActScav;
                }
            }
            // No command set something went wrong
            yield break;
        }
        void ScavengeBuilding()
        {
            // When command above is fired mark the building for deconstruction
            var designator = new Designator_Deconstruct();
            designator.DesignateThing(this);
        }
    }
}
