using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace RimworldAscension
{
    public class CompCaravan : ThingComp
    {
        public List<Thing> cargo = new List<Thing>();

        public Graphic cartTexture;
        public Graphic wheelTexture;
        public Graphic harnessTexture;

        // half of the quater of the cell
        public const float wheelRadius = 0.125f;

        // current spin degree
        public float rotor = 0;

        public float num = 0;
        public float num1 = 0;
        public float num2 = 0;
        public float num3 = 0;
        public float num4 = 0;
        public float num5 = 0;

        public CompCaravan_Properties Properties
        {
            get
            {
                return (CompCaravan_Properties)props;
            }
        }

        public bool AliveAndWell(Pawn pawn)
        {
            if (!pawn.SpawnedInWorld || pawn.Downed || pawn.Dead)
            {
                return false;
            }
            else
                return true;
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();

            LoadTextures();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (cargo.Count != 0)
            {
                SetTexturesPosition();

                Pawn pawn = this.parent as Pawn;
                if (pawn.pather.Moving)
                {
                    // rotate rotor by parent's move speed value
                    float degree = pawn.GetStatValue(StatDefOf.MoveSpeed) * 2 / (GenTicks.TicksPerRealtimeSecond * wheelRadius);
                    RotateWheelByDegree(degree);
                }
            }
        }

        public void RotateWheelByDegree(float degree)
        {
            // nullify rotor counter if wheel made whole spin
            if (rotor % 360 == 0)
            {
                rotor = 0;
            }
            if (this.parent.Rotation == Rot4.East)
            {
                rotor += degree;
            }
            else if (this.parent.Rotation == Rot4.West)
            {

                rotor -= degree;
            }
            else
            {
                rotor = 0;
            }
        }
        
        public void SetTexturesPosition()
        {
            if (this.parent.Rotation == Rot4.South)
            {
                num1 = 0f;
                num2 = -2f;
                num3 = num2 + (-0.5f);
                num4 = 0;
                num5 = 0;
            }
            else if (this.parent.Rotation == Rot4.West || this.parent.Rotation == Rot4.East)
            {
                num1 = -1f;
                num2 = 0;
                num3 = 0.5f;
                num4 = -0.3f;
                num5 = (this.parent.Rotation == Rot4.West) ? -0.35f : 0.35f;
            }
            else
            {
                num1 = 0;
                num2 = 0;
                num3 = -0.5f;
                num4 = 0;
                num5 = 0;
            }
        }

        public override void PostDraw()
        {
            if (cargo.Count != 0)
            {
                if (AliveAndWell(this.parent as Pawn))
                {
                    //Harness
                    Matrix4x4 matrix = default(Matrix4x4);
                    Vector3 s = new Vector3(3f, 0.5f, 3f);
                    matrix.SetTRS(this.parent.DrawPos + Altitudes.AltIncVect, num.ToQuat(), s);
                    Graphics.DrawMesh((this.parent.Rotation == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix, harnessTexture.MatAt(base.parent.Rotation), 0);

                    //Cart
                    Matrix4x4 matrix2 = default(Matrix4x4);
                    Vector3 offset = new Vector3(0f, 0f + num2, -1f + num1).RotatedBy(this.parent.Rotation.AsAngle);
                    Vector3 s1 = new Vector3(3f, 0.5f, 3f);
                    matrix2.SetTRS(this.parent.DrawPos + offset + Altitudes.AltIncVect, num.ToQuat(), s1);
                    Graphics.DrawMesh((this.parent.Rotation == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix2, cartTexture.MatAt(base.parent.Rotation), 0);

                    //Wheels
                    Matrix4x4 matrix3 = default(Matrix4x4);
                    Vector3 s2 = new Vector3(3f, offset.y, 3f);
                    Vector3 offset2 = new Vector3(0 + num5, offset.y + num3, -1f + num1 + num4).RotatedBy(this.parent.Rotation.AsAngle);
                    matrix3.SetTRS(this.parent.DrawPos + offset2 + Altitudes.AltIncVect, rotor.ToQuat(), s2);
                    Graphics.DrawMesh((this.parent.Rotation == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix3, wheelTexture.MatAt(base.parent.Rotation), 0);
                }
            }
        }

        public void LoadTextures()
        {
            if (!string.IsNullOrEmpty(Properties.cartTexturePath) && !string.IsNullOrEmpty(Properties.wheelTexturePath) && !string.IsNullOrEmpty(Properties.harnessTexturePath))
            {
                cartTexture = GraphicDatabase.Get<Graphic_Multi>(Properties.cartTexturePath);
                wheelTexture = GraphicDatabase.Get<Graphic_Multi>(Properties.wheelTexturePath);
                harnessTexture = GraphicDatabase.Get<Graphic_Multi>(Properties.harnessTexturePath);
            }
            else
                Log.Error("Missing texture path data");
        }
    }
}
