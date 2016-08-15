using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    [StaticConstructorOnStartup]
    public class CompCaravan : ThingComp
    {
        public static Graphic cartEmptyTexture = GraphicDatabase.Get<Graphic_Multi>("Things/Special/CaravanCart/CartEmpty", ShaderDatabase.Transparent, new Vector2(2f, 2f), Color.white);
        public static Graphic cartFullTexture = GraphicDatabase.Get<Graphic_Multi>("Things/Special/CaravanCart/CartFull", ShaderDatabase.Transparent, new Vector2(2f, 2f), Color.white);
        public static Graphic wheelTexture = GraphicDatabase.Get<Graphic_Multi>("Things/Special/CaravanCart/Wheel", ShaderDatabase.Transparent, new Vector2(2f, 2f), Color.white);
        public static Graphic harnessTexture = GraphicDatabase.Get<Graphic_Multi>("Things/Special/CaravanCart/Harness", ShaderDatabase.Transparent, new Vector2(2f, 2f), Color.white);

        public Pawn carrier;

        // destroys the cart once carrier is down
        public bool broken;

        // half of the quater of the cell
        public const float wheelRadius = 0.0256f;

        // current spin degree
        public float rotor;

        public float num = 0;
        public float num1;
        public float num2;
        public float num3;
        public float num4;
        public float num5;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            carrier = parent as Pawn;
        }

        public bool SpawnedAndWell => carrier.Spawned && !carrier.Downed;

        public override void CompTick()
        {
            base.CompTick();

            if (SpawnedAndWell)
            {
                SetTexturesPosition();
                if (carrier.pather.Moving)
                {
                    // rotate rotor by parent's move speed value
                    var degree = carrier.GetStatValue(StatDefOf.MoveSpeed) / (GenDate.SecondsToTicks(1) * wheelRadius);
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

            if (parent.Rotation == Rot4.East)
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
            if (parent.Rotation == Rot4.South)
            {
                num1 = 0f;
                num2 = -0.1f;
                num3 = -0.01f;
                num4 = 0;
                num5 = 0;
            }
            else if (parent.Rotation == Rot4.West || parent.Rotation == Rot4.East)
            {
                num1 = -0.4f;
                num2 = -0.01f;
                num3 = 0.01f;
                num4 = -0.7f;
                num5 = parent.Rotation == Rot4.West ? -0.35f : 0.35f;
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
            if (SpawnedAndWell)
            {
                //Harness
                var matrix = default(Matrix4x4);
                var s = new Vector3(3f, 0.05f, 3f);
                matrix.SetTRS(parent.DrawPos + Altitudes.AltIncVect, num.ToQuat(), s);
                Graphics.DrawMesh(parent.Rotation == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix, harnessTexture.MatAt(parent.Rotation), 0);

                //Cart
                var matrix2 = default(Matrix4x4);
                var offset = new Vector3(0f, s.y + num2, -1f + num1).RotatedBy(parent.Rotation.AsAngle);
                matrix2.SetTRS(parent.DrawPos + offset + Altitudes.AltIncVect, num.ToQuat(), s);
                Graphics.DrawMesh(parent.Rotation == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix2,
                    carrier.inventory.container.Count > 0
                        ? cartFullTexture.MatAt(parent.Rotation)
                        : cartEmptyTexture.MatAt(parent.Rotation), 0);

                //Wheels
                var matrix3 = default(Matrix4x4);
                var s2 = new Vector3(3f, offset.y, 3f);
                var offset2 = new Vector3(0 + num5, offset.y + num3, -1f + num1 + num4).RotatedBy(parent.Rotation.AsAngle);
                if (parent.Rotation == Rot4.West || parent.Rotation == Rot4.East)
                {
                    matrix3.SetTRS(parent.DrawPos + offset2 + Altitudes.AltIncVect, rotor.ToQuat(), s2);
                    Graphics.DrawMesh(parent.Rotation == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix3, wheelTexture.MatAt(parent.Rotation), 0);
                }
                else
                {
                    matrix3.SetTRS(parent.DrawPos + offset2 + Altitudes.AltIncVect, Rot4.North.AsQuat, s2);
                    Graphics.DrawMesh(parent.Rotation == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix3, rotor % 30 == 0 ? wheelTexture.MatBack : wheelTexture.MatFront, 0);
                }
            }
        }
    }

    public class CompCaravan_Properties : CompProperties
    {
        public CompCaravan_Properties()
        {
            compClass = typeof(CompCaravan);
        }
    }
}
