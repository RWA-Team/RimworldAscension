using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public class TomahawkThrown : Bullet
    {
        private static readonly SoundDef TomahawkHit = SoundDef.Named("Pawn_Melee_BigBash_HitPawn");
        private const int Rspeed = 15;
        private float CurRotationInt;

        public override void Tick()
        {
            base.Tick();
            CurRotationInt -= Rspeed;
        }

        public override void Draw()
        {
            var matrix = default(Matrix4x4);
            var s = new Vector3(1f, 1f, 1f);
            matrix.SetTRS(DrawPos + Altitudes.AltIncVect, CurRotationInt.ToQuat(), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, Graphic.MatAt(Rotation), 0);
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                TomahawkHit.PlayOneShot(Position);
            }
        }
    }
}