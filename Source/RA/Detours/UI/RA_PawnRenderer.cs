using UnityEngine;
using Verse;

namespace RA
{
    public class RA_PawnRenderer : PawnRenderer
    {
        public RA_PawnRenderer(Pawn pawn) : base(pawn)
        {
        }

        // draws hands on equipment, if corresponding Comp is specified
        public new void DrawEquipmentAiming(Thing equipment, Vector3 drawLoc, float aimAngle)
        {
            var num = aimAngle - 90f;
            Mesh mesh;
            var flipped = false;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += equipment.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= equipment.def.equippedAngleOffset;
                flipped = true;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += equipment.def.equippedAngleOffset;
            }
            num %= 360f;
            var graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            var matSingle = graphic_StackCount != null
                ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                : equipment.Graphic.MatSingle;
            Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);

            // draws hands on equipment, if corresponding Comp is specified
            var compHandsDrawer = equipment.TryGetComp<CompHandsDrawer>();
            if (compHandsDrawer != null)
            {
                var handGraphic = GraphicDatabase.Get<Graphic_Single>("Overlays/Hand", ShaderDatabase.CutoutSkin, Vector2.one, graphics.pawn.story.SkinColor).MatSingle;

                float offsetX, offsetY;
                if (compHandsDrawer.FirstHandPosition != Vector3.zero)
                {
                    offsetX = flipped
                        ? -compHandsDrawer.FirstHandPosition.x
                        : compHandsDrawer.FirstHandPosition.x;
                    offsetY = compHandsDrawer.FirstHandPosition.z;

                    Graphics.DrawMesh(mesh,
                        drawLoc + new Vector3(offsetX, 0.1f, offsetY).RotatedBy(num),
                        Quaternion.AngleAxis(num, Vector3.up), handGraphic, 0);
                }
                if (compHandsDrawer.SecondHandPosition != Vector3.zero)
                {
                    offsetX = flipped
                        ? -compHandsDrawer.SecondHandPosition.x
                        : compHandsDrawer.SecondHandPosition.x;
                    offsetY = compHandsDrawer.SecondHandPosition.z;

                    Graphics.DrawMesh(mesh,
                        drawLoc + new Vector3(offsetX, 0.1f, offsetY).RotatedBy(num),
                        Quaternion.AngleAxis(num, Vector3.up), handGraphic, 0);
                }
            }
        }
    }
}