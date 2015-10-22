using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace RA
{
    public class CompCaravan : ThingComp
    {
        public List<Thing> cargo = new List<Thing>();

        public Graphic cartEmptyTexture;
        public Graphic cartFullTexture;
        public Graphic wheelTexture;
        public Graphic harnessTexture;

        // destroys the cart once carrier is down
        public bool broken = false;

        // half of the quater of the cell
        public const float wheelRadius = 0.0256f;

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

        public bool SpawnedAndWell(Pawn pawn)
        {
            if (!pawn.SpawnedInWorld || pawn.Downed)
                return false;
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

            if (SpawnedAndWell(this.parent as Pawn))
            {
                SetTexturesPosition();

                Pawn pawn = this.parent as Pawn;
                if (pawn.pather.Moving)
                {
                    // rotate rotor by parent's move speed value
                    float degree = pawn.GetStatValue(StatDefOf.MoveSpeed) / (GenTicks.TicksPerRealtimeSecond * wheelRadius);
                    RotateWheelByDegree(degree);
                }
            }
            else if (broken == false)
            {
                broken = true;
                PostDestroy();
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
            else
            {
                rotor -= degree;
            }
        }
        
        public void SetTexturesPosition()
        {
            if (this.parent.Rotation == Rot4.South)
            {
                num1 = 0f;
                num2 = -0.1f;
                num3 = -0.01f;
                num4 = 0;
                num5 = 0;
            }
            else if (this.parent.Rotation == Rot4.West || this.parent.Rotation == Rot4.East)
            {
                num1 = -0.4f;
                num2 = -0.01f;
                num3 = 0.01f;
                num4 = -0.7f;
                num5 = (this.parent.Rotation == Rot4.West) ? -0.35f : 0.35f;
            }
            else
            {
                num1 = 0;
                num2 = 0;
                num3 = -0.01f;
                num4 = 0;
                num5 = 0;
            }
        }

        // draw cart, harness and wheels overlay over caravan muffalo if it's in good condition
        public override void PostDraw()
        {
            if (!broken)
            {
                //Harness
                Matrix4x4 matrix = default(Matrix4x4);
                Vector3 s = new Vector3(3f, 0.05f, 3f);
                matrix.SetTRS(this.parent.DrawPos + Altitudes.AltIncVect, num.ToQuat(), s);
                Graphics.DrawMesh((this.parent.Rotation == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix, harnessTexture.MatAt(base.parent.Rotation), 0);

                //Cart
                Matrix4x4 matrix2 = default(Matrix4x4);
                Vector3 offset = new Vector3(0f, s.y + num2, -1f + num1).RotatedBy(this.parent.Rotation.AsAngle);
                matrix2.SetTRS(this.parent.DrawPos + offset + Altitudes.AltIncVect, num.ToQuat(), s);
                if (cargo != null && cargo.Count > 0)
                    Graphics.DrawMesh((this.parent.Rotation == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix2, cartFullTexture.MatAt(base.parent.Rotation), 0);
                else
                    Graphics.DrawMesh((this.parent.Rotation == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix2, cartEmptyTexture.MatAt(base.parent.Rotation), 0);

                //Wheels
                Matrix4x4 matrix3 = default(Matrix4x4);
                Vector3 s2 = new Vector3(3f, offset.y, 3f);
                Vector3 offset2 = new Vector3(0 + num5, offset.y + num3, -1f + num1 + num4).RotatedBy(this.parent.Rotation.AsAngle);
                if (this.parent.Rotation == Rot4.West || this.parent.Rotation == Rot4.East)
                {
                    matrix3.SetTRS(this.parent.DrawPos + offset2 + Altitudes.AltIncVect, rotor.ToQuat(), s2);
                    Graphics.DrawMesh((this.parent.Rotation == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix3, wheelTexture.MatAt(base.parent.Rotation), 0);
                }
                else
                {
                    matrix3.SetTRS(this.parent.DrawPos + offset2 + Altitudes.AltIncVect, Rot4.North.AsQuat, s2);
                    Graphics.DrawMesh((this.parent.Rotation == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix3, (rotor % 30 == 0) ? wheelTexture.MatBack : wheelTexture.MatFront, 0);
                }
            }
        }

        public override void PostDestroy(DestroyMode mode = DestroyMode.Kill)
        {
            foreach (Thing thing in cargo)
            {
                Thing newThing;
                GenDrop.TryDropSpawn(thing, this.parent.Position, ThingPlaceMode.Near, out newThing);
            }
            cargo.Clear();
        }

        public void LoadTextures()
        {
            if (!string.IsNullOrEmpty(Properties.cartEmptyTexturePath) && !string.IsNullOrEmpty(Properties.cartFullTexturePath) && !string.IsNullOrEmpty(Properties.wheelTexturePath) && !string.IsNullOrEmpty(Properties.harnessTexturePath))
            {
                cartEmptyTexture = GraphicDatabase.Get<Graphic_Multi>(Properties.cartEmptyTexturePath, ShaderDatabase.Transparent, new Vector2(2f, 2f), Color.white);
                cartFullTexture = GraphicDatabase.Get<Graphic_Multi>(Properties.cartFullTexturePath, ShaderDatabase.Transparent, new Vector2(2f, 2f), Color.white);
                wheelTexture = GraphicDatabase.Get<Graphic_Multi>(Properties.wheelTexturePath, ShaderDatabase.Transparent, new Vector2(2f, 2f), Color.white);
                harnessTexture = GraphicDatabase.Get<Graphic_Multi>(Properties.harnessTexturePath, ShaderDatabase.Transparent, new Vector2(2f, 2f), Color.white);
            }
            else
                Log.Error("Missing texture path data");
        }
    }
}
