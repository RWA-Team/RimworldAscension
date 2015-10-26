using UnityEngine;
using System.Collections.Generic;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RA
{
    public class ShipWreckCrashing : DropPodCrashing
    {
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            ticksToImpact = Rand.RangeInclusive(120, 200);
            impactShakeStrength = 0.013f;
            crater = (ThingWithComps)ThingMaker.MakeThing(ThingDef.Named("CraterLarge"));
            wreck = (DropPodCrashed)ThingMaker.MakeThing(ThingDef.Named("ShipWreckCrashed"));
            wreck.cargo = this.cargo;
        }

        public override void Draw()
        {
            // Set up a matrix
            Matrix4x4 matrix = default(Matrix4x4);
            // Set up a vector
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
            Graphics.DrawMesh(MeshPool.plane10, matrix, ShipWreckCrashing.ShadowMat, 0);
        }
    }
}
