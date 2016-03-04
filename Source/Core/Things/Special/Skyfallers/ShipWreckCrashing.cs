using UnityEngine;
using Verse;

namespace RA
{
    public class ShipWreckCrashing : DropPodCrashing
    {
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            ticksToImpact = Rand.RangeInclusive(120, 200);
            wreck = (DropPodCrashed)ThingMaker.MakeThing(ThingDef.Named("ShipWreckCrashed"));
            wreck.cargo = cargo;
        }

        public override void Draw()
        {
            // Set up a matrix
            var matrix = default(Matrix4x4);
            // Set up a vector
            var s = new Vector3(6f, 0f, 3f);
            // Adjust the angle of the texture to 45 degrees
            matrix.SetTRS(DrawPos + Altitudes.AltIncVect, Quaternion.AngleAxis(45, Vector3.up), s);
            // Draw new angled graphic
            Graphics.DrawMesh(MeshPool.plane10, matrix, Graphic.MatAt(Rotation), 0);
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            // Shadow draw code, Call base
            base.DrawAt(drawLoc);
            // Setup center point
            var pos = this.TrueCenter();
            // Adjust for altitude
            pos.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
            // contract shadow as altitude falls
            var num = 2f + ticksToImpact / 100f;
            var s = new Vector3(num, 1f, num);
            var matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.AngleAxis(135f, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, ShadowMat, 0);
        }
    }
}
