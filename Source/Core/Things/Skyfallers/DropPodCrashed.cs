using System;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace RA
{
    public class DropPodCrashed : Thing
    {
        public const int NumStartingMealsPerColonist = 10;
        public const int NumStartingMedPacksPerColonist = 1;
        public static readonly SoundDef OpenSound = SoundDef.Named("DropPodOpen"); // Open sound

        public bool deployed; // whether skyfaller has opened yet

        public DropPodInfo cargo; // Drop pods contents and config
        public List<IntVec3> damagedCells = new List<IntVec3>(); // list of smoke and spark generators

        public override void SpawnSetup()
        {
            // Do base setup
            base.SpawnSetup();

            // probability for the cell to throw smoke motes, halved  with each iteration
            float chance = 1f;
            foreach (IntVec3 cell in GenAdj.CellsOccupiedBy(this))
            {
                if (chance > new System.Random().NextDouble())
                    damagedCells.Add(cell);
                chance /= 2;
            }
        }

        public override void Tick()
        {
            if (!deployed)
                if (cargo.openDelay-- <= 0)
                {
                    Deploy();
                    deployed = true;
                }

            foreach (IntVec3 damagedCell in damagedCells)
            {
                // Throw smoke mote
                SpecialMotes.ThrowSmokeBlack(damagedCell.ToVector3(), 0.5f);
            }
        }

        // used to spawn things after delay automatically
        public virtual void Deploy()
        {
            // loop over all contents
            foreach (Thing thing in cargo.containedThings)
            {
                // Place thing in world
                GenPlace.TryPlaceThing(thing, this.Position, ThingPlaceMode.Near);
                // If its a pawn
                Pawn pawn = thing as Pawn;
                // if its a humanlike pawn
                if (pawn != null && pawn.RaceProps.Humanlike)
                {
                    // Record a tale
                    TaleRecorder.RecordTale(TaleDef.Named("LandedInPod"), new object[]
                    {
                        pawn
                    });
                }
                // Kill pawns due to impact
                HealthUtility.GiveInjuriesToKill(pawn);
            }
            // All contents dealt with, clear list
            this.cargo.containedThings.Clear();
            // Play open sound
            DropPodCrashed.OpenSound.PlayOneShot(base.Position);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            // Do base gizmos if required
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            // Setup a new command
            Command_Action gizmo_Scavenge = new Command_Action
            {
                // Command icon
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Scavenge"),
                // Command label
                defaultLabel = "Scavenge",
                // Command description
                defaultDesc = "Try to find any useful things inside this wreck",
                // Command sound when activated
                activateSound = SoundDef.Named("Click"),
                // Action to call
                action = new Action(Scavenge)
            };
            yield return gizmo_Scavenge;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            Action action = () =>
            {
                pawn.drafter.TakeOrderedJob(new Job(JobDefOf.Deconstruct, this));
            };
            yield return new FloatMenuOption("Scavenge the " + this.Label, action);
        }

        public void Scavenge()
        {
            this.SetFaction(Faction.OfColony);
            Find.DesignationManager.AddDesignation(new Designation(this, DesignationDefOf.Deconstruct));
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Call base destory method
            base.Destroy(mode);

            // Check destroy mod passed
            if (mode == DestroyMode.Deconstruct)
            {
                SpawnScavengebles();
            }
        }

        // used to spawn things after scavenge action
        public virtual void SpawnScavengebles()
        {
            // Generate some steel slags
            for (int j = 0; j < 3; j++)
            {
                Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
                GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near);
            }

            // Generate some meal packs
            Thing thing2 = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
            thing2.stackCount = NumStartingMealsPerColonist;
            GenPlace.TryPlaceThing(thing2, base.Position, ThingPlaceMode.Near);

            // Generate some medicine
            Thing thing3 = ThingMaker.MakeThing(ThingDefOf.Medicine);
            thing3.stackCount = NumStartingMedPacksPerColonist;
            GenPlace.TryPlaceThing(thing3, base.Position, ThingPlaceMode.Near);
        }

        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();

            // Save tickstoimpact to save file
            Scribe_Values.LookValue(ref deployed, "deployed");
            Scribe_Deep.LookDeep(ref cargo, "info", new object[0]);
            Scribe_Collections.LookList(ref damagedCells, "damagedCells", LookMode.Value);
        }
    }
}