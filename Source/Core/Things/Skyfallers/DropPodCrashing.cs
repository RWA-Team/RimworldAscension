using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RA
{
    public class DropPodCrashing : Thing
    {
        public static readonly SoundDef LandSound = SoundDef.Named("DropPodFall"); // Landing sound
        public static readonly Material ShadowMat = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent); // The shadow graphic of the droppod

        public int ticksToImpact; // Ticks until pod hits the ground
        public bool soundPlayed; // Whether sound has been played or not

        public DropPodCrashed wreck;
        public DropPodInfo cargo;

        public override void SpawnSetup()
        {
            // Do base setup
            base.SpawnSetup();

            ticksToImpact = Rand.RangeInclusive(120, 200);
            wreck = (DropPodCrashed)ThingMaker.MakeThing(ThingDef.Named("DropPodCrashed"));
            wreck.cargo = this.cargo;
        }

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

        public override void Tick()
        {
            // If the pod is inflight, slight ending delay here to stop smoke motes covering the crash
            if (this.ticksToImpact > 8)
            {
                // Throw some smoke and fire glow trails
                MoteThrower.ThrowSmoke(DrawPos, 2f);
                MoteThrower.ThrowFireGlow(DrawPos.ToIntVec3(), 1.5f);
            }
            // Drop the ticks to impact
            this.ticksToImpact--;
            // If we have hit the ground
            if (this.ticksToImpact <= 0)
            {
                // Hit the ground
                // explosion damage multiplier 1f ~ 100 damage
                SkyfallerUtility.Impact(this, wreck, 20f);
            }
            // If we havent already played the sound and we are low enough
            if (!this.soundPlayed && this.ticksToImpact < 100)
            {
                // Set the bool to true so sound doesnt play again
                this.soundPlayed = true;
                // Play the sound
                DropPodCrashing.LandSound.PlayOneShot(base.Position);
            }
        }
        public override void Draw()
        {
            // Set up a matrix
            Matrix4x4 matrix = default(Matrix4x4);
            // Set up a vector
            Vector3 s = new Vector3(1.9f, 0f, 1.9f);
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
            float num = 2f + ticksToImpact / 100f;
            Vector3 s = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.AngleAxis(135f, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, DropPodCrashing.ShadowMat, 0);
        }

        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();

            // Save tickstoimpact to save file
            Scribe_Values.LookValue(ref ticksToImpact, "ticksToImpact");
            Scribe_Deep.LookDeep(ref cargo, "cargo", new object[0]);
        }
    }
}
