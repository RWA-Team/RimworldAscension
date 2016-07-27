using UnityEngine;
using Verse;

namespace RA
{
    [StaticConstructorOnStartup]
    public static class RA_GenDraw
    {
        public static readonly Material AimPieMaterial = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 1f, 0.15f));

        public static void DrawShootingCone(Thing shooter, TargetInfo target, int degreesWide, float range)
        {
            var facing = 0f;
            if (target.Cell != shooter.Position)
            {
                facing = target.Thing != null
                    ? (target.Thing.DrawPos - shooter.Position.ToVector3Shifted()).AngleFlat()
                    : (target.Cell - shooter.Position).AngleFlat;
            }
            var center = shooter.DrawPos + new Vector3(0f, range, 0f);

            var s = new Vector3(range, 1f, range);
            var matrix = default(Matrix4x4);
            center += Quaternion.AngleAxis(facing, Vector3.up) * Vector3.forward * 0.8f;
            matrix.SetTRS(center, Quaternion.AngleAxis(facing + degreesWide / 2 - 90f, Vector3.up), s);

            Graphics.DrawMesh(MeshPool.pies[degreesWide], matrix, AimPieMaterial, 0);
        }
    }
}