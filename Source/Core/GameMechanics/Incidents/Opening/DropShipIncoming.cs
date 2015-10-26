using UnityEngine;
using System.Collections.Generic;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RA
{
    public class DropShipIncoming : Thing
    {
        public DropShipInfo contents; // Contents and config of the ship
        protected int ticksToImpact = 120; // Ticks until ship hits the ground
        private bool soundPlayed; // Whether sound has been played or not
        private static readonly SoundDef LandSound = SoundDef.Named("DropPodFall"); // Landing sound
        private static readonly Material ShadowMat = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent); // The shadow graphic of the ship

        public override Vector3 DrawPos
        {
            get
            {
                // Adjust the vector based on things altitude
                Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
                // Get a float from tickstoimpact
                float num = (float)(this.ticksToImpact * this.ticksToImpact) * 0.01f;
                // set offsets
                result.x -= num * 0.4f;
                result.z += num * 0.6f;
                // Return
                return result;
            }
        }

        public override void SpawnSetup()
        {
            // Do base setup
            base.SpawnSetup();
            // randomise the ticks to impact so all pods dont fall at exactly the same time
            this.ticksToImpact = Rand.RangeInclusive(120, 200);
        }

        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();
            // Save tickstoimpact to save file
            Scribe_Values.LookValue<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
            // Save pod contents to save file
            Scribe_Deep.LookDeep<DropShipInfo>(ref this.contents, "contents", new object[0]);
        }

        public override void Tick()
        {
            // If the pod is inflight, slight ending delay here to stop smoke motes covering the crash
            if (this.ticksToImpact > 8)
            {
                // Throw some smoke and fire glow trails
                MoteThrower.ThrowSmoke(DrawPos, 3f);
                MoteThrower.ThrowFireGlow(DrawPos.ToIntVec3(), 1.5f);
            }
            // Drop the ticks to impact
            this.ticksToImpact--;
            // If we have hit the ground
            if (this.ticksToImpact <= 0)
            {
                // Hit the ground
                this.ShipImpact();
            }
            // If we havent already played the sound and we are low enough
            if (!this.soundPlayed && this.ticksToImpact < 100)
            {
                // Set the bool to true so sound doesnt play again
                this.soundPlayed = true;
                // Play the sound
                DropShipIncoming.LandSound.PlayOneShot(base.Position);
            }
        }

        public override void Draw()
        {
            //Set up a matrix
            Matrix4x4 matrix = default(Matrix4x4);
            //Set up a vector
            Vector3 s = new Vector3(6f, 0f, 3f);
            // Adjust the angle of the texture to 45 degrees
            matrix.SetTRS(DrawPos + Altitudes.AltIncVect, Quaternion.AngleAxis(45, Vector3.up), s);
            // Draw new angled graphic
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.Graphic.MatAt(this.Rotation, null), 0);
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            // Shadow draw code, Call base
            base.DrawAt(drawLoc);
            // Setup center point
            Vector3 pos = this.TrueCenter();
            // Adjust for altitude
            pos.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
            // contract shadow as altitude falls
            float num = 2f + (float)this.ticksToImpact / 100f;
            Vector3 s = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.AngleAxis(135f, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, DropShipIncoming.ShadowMat, 0);
        }

        private void ShipImpact()
        {
            // Loop a few times
            for (int i = 0; i < 6; i++)
            {
                // Throw some dust puffs
                Vector3 loc = base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f);
                MoteThrower.ThrowDustPuff(loc, 1.2f);
            }
            // Throw a quick flash
            MoteThrower.ThrowLightningGlow(base.Position.ToVector3Shifted(), 2f);
            // Create a crater
            ThingWithComps crater = (ThingWithComps)ThingMaker.MakeThing(ThingDef.Named("CraterLarge"), null);
            // Create a crashed ship
            DropShipCrashed dropShip = (DropShipCrashed)ThingMaker.MakeThing(ThingDef.Named("DropShipCrashed"), null);
            // Set contents into the new crashed droppod
            dropShip.info = this.contents;
            // Spawn the crater
            GenSpawn.Spawn(crater, Position, base.Rotation);
            // Spawn the crashed ship part
            GenSpawn.Spawn(dropShip, base.Position, base.Rotation);
            foreach (IntVec3 current in GenRadial.RadialCellsAround(crater.Position, crater.def.size.x / 2, true))
            {
                // List all things found in these cells
                List<Thing> list = Find.ThingGrid.ThingsListAt(current);
                // Reverse iterate through the things so we can destroy without breaking the pointer
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    // If its a plant, filth, or an item
                    if (list[i].def.category == ThingCategory.Plant || list[i].def.category == ThingCategory.Filth || list[i].def.category == ThingCategory.Item)
                    {
                        // Destroy it
                        list[i].Destroy();
                    }
                }
            }
            // Get the roof def
            RoofDef roof = base.Position.GetRoof();
            // If there was actually a roof
            if (roof != null)
            {
                // If we can punch through
                if (!roof.soundPunchThrough.NullOrUndefined())
                {
                    // Play punch sound
                    roof.soundPunchThrough.PlayOneShot(base.Position);
                }
                // If the roof def is to leave filth
                if (roof.filthLeaving != null)
                {
                    // Drop some filth
                    for (int j = 0; j < 3; j++)
                    {
                        FilthMaker.MakeFilth(base.Position, roof.filthLeaving, 1);
                    }
                }
            }
            // Do a bit of camera shake for added effect
            CameraShaker.DoShake(0.013f);
            // Fire an explosion with motes
            GenExplosion.DoExplosion(base.Position, 2f, DamageDefOf.Bomb, null, null, null);
            CellRect cellRect = CellRect.CenteredOn(base.Position, 2);
            cellRect.ClipInsideMap();
            for (int i = 0; i < 5; i++)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                MoteThrower.ThrowFireGlow(DrawPos.ToIntVec3(), 1.5f);
            }
            // Destroy pod
            this.Destroy(DestroyMode.Vanish);
        }
    }
}
