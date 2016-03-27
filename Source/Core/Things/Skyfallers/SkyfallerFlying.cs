using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public class SkyfallerFlying : Thing
    {
        public static readonly SoundDef LandSound = SoundDef.Named("DropPodFall"); // Landing sound
        public static readonly Material ShadowMat = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent); // The shadow graphic of the droppod

        // ticks after spaning to impact
        public int ticksToImpact = Rand.RangeInclusive(120, 300);
        public bool impactSoundPlayed;
        
        // flying rotation params
        public int rotSpeed = 10;
        public int rotAngle;

        // needs to be assigned in inherited SpawnSetup()
        public Thing impactResultThing;

        // ajust the fying position according to ticksToImpact
        public override Vector3 DrawPos
        {
            get
            {
                // Adjust the vector based on things altitude
                var result = Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
                // Get a float from tickstoimpact
                var num = ticksToImpact * ticksToImpact * 0.01f;
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
            if (ticksToImpact > 8)
            {
                // Throw some smoke and fire glow trails
                MoteThrower.ThrowSmoke(DrawPos, 2f);
                MoteThrower.ThrowFireGlow(DrawPos.ToIntVec3(), 1.5f);
            }
            // Drop the ticks to impact
            ticksToImpact--;
            rotAngle = (rotAngle + rotSpeed) % 360;
            // If we have hit the ground
            if (ticksToImpact <= 0)
            {
                // Hit the ground
                // explosion damage multiplier 1f ~ 100 damage
                SkyfallerUtility.Impact(this, impactResultThing, 20f);
            }
            // If we havent already played the sound and we are low enough
            if (!impactSoundPlayed && ticksToImpact < 100)
            {
                // Set the bool to true so sound doesnt play again
                impactSoundPlayed = true;
                // Play the sound
                LandSound.PlayOneShot(Position);
            }
        }

        public override void Draw()
        {
            var matrix = default(Matrix4x4);
            var SizeVector = new Vector3(Graphic.data.drawSize.x, 0f, Graphic.data.drawSize.y);
            // rotate skyfaller texture while flying
            matrix.SetTRS(DrawPos + Altitudes.AltIncVect, Quaternion.AngleAxis(rotAngle, Vector3.up), SizeVector);
            // Draw new angled graphic
            Graphics.DrawMesh(MeshPool.plane10, matrix, Graphic.MatAt(Rotation), 0);
        }

        // saves importand data to save file
        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.LookValue(ref ticksToImpact, "ticksToImpact");
            Scribe_Values.LookValue(ref rotAngle, "rotAngle");
        }
    }
}