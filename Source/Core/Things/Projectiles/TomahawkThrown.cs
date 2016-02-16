using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RA
{
    public class TomahawkThrown : Bullet
    {
        private static readonly SoundDef TomahawkHit = SoundDef.Named("Pawn_Melee_BigBash_HitPawn");
        private float CurRotationInt = 0f;
        private int Rspeed = 15;

        public override void Tick()
        {
            base.Tick();
            this.CurRotationInt -= (float)this.Rspeed;
        }

        public override void Draw()
        {
            Matrix4x4 matrix = default(Matrix4x4);
            Vector3 s = new Vector3(1f, 1f, 1f);
            matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, Gen.ToQuat(CurRotationInt), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.Graphic.MatAt(base.Rotation, null), 0);
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                TomahawkHit.PlayOneShot(base.Position);
            }
        }
    }
}