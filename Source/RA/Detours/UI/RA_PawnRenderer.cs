using System.Reflection;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_PawnRenderer : PawnRenderer
    {
        public static FieldInfo infoPawn;

        public RA_PawnRenderer(Pawn Pawn) : base(Pawn)
        {
        }

        public Pawn Pawn => Initializer.GetHiddenValue(typeof(PawnRenderer), this, "pawn", infoPawn) as Pawn;

        public void DrawEquipment(Vector3 rootLoc)
        {
            if (Pawn.Dead || !Pawn.Spawned)
            {
                return;
            }
            if (Pawn.equipment?.Primary == null)
            {
                return;
            }
            if (Pawn.CurJob != null && Pawn.CurJob.def.neverShowWeapon)
            {
                return;
            }
            rootLoc.y += 0.0449999981f;
            var stance_Busy = Pawn.stances.curStance as Stance_Busy;
            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                Vector3 targetPos;
                targetPos = stance_Busy.focusTarg.HasThing
                    ? stance_Busy.focusTarg.Thing.DrawPos
                    : stance_Busy.focusTarg.Cell.ToVector3Shifted();
                var angle = 0f;
                if ((targetPos - Pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    angle = (targetPos - Pawn.DrawPos).AngleFlat();
                }
                var b = new Vector3(0f, 0f, 0.4f).RotatedBy(angle);

                // adjusts aiming angle position, if specified
                var compHandsDrawer = Pawn.equipment.Primary.TryGetComp<CompHandsDrawer>();
                if (compHandsDrawer != null) angle += compHandsDrawer.AimingAngleOffset;

                DrawEquipmentAiming(Pawn.equipment.Primary, rootLoc + b, angle);
            }
            else if (CarryWeaponOpenly())
            {
                if (Pawn.Rotation == Rot4.North)
                {
                    var drawLoc = rootLoc;
                    DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc, 143f);
                }
                else if (Pawn.Rotation == Rot4.South)
                {
                    var drawLoc = rootLoc + new Vector3(0f, 0f, -0.22f);
                    DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc, 143f);
                }
                else if (Pawn.Rotation == Rot4.East)
                {
                    var drawLoc2 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
                    DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc2, 143f);
                }
                else if (Pawn.Rotation == Rot4.West)
                {
                    var drawLoc3 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
                    DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc3, 217f);
                }
            }
        }

        // vanilla is hidden
        public bool CarryWeaponOpenly()
        {
            return (Pawn.carrier?.CarriedThing == null) && (Pawn.Drafted || (Pawn.CurJob != null && Pawn.CurJob.def.alwaysShowWeapon) || (Pawn.mindState.duty != null && Pawn.mindState.duty.def.alwaysShowWeapon));
        }

        // draws hands on equipment, if corresponding Comp is specified
        public new void DrawEquipmentAiming(Thing equipment, Vector3 drawLoc, float aimAngle)
        {
            // used to draw weapon beneath the Pawn when facing north and west
            var drawingOffset = Pawn.Rotation == Rot4.West || Pawn.Rotation == Rot4.North
                ? new Vector3(0, -0.5f, 0)
                : Vector3.zero;
            var turnAngle = aimAngle - 90f;
            Mesh mesh;
            var flipped = false;

            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                turnAngle += equipment.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                turnAngle -= 180f;
                turnAngle -= equipment.def.equippedAngleOffset;
                flipped = true;
            }
            else
            {
                mesh = MeshPool.plane10;
                turnAngle += equipment.def.equippedAngleOffset;
            }
            turnAngle %= 360f;

            var graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            var weaponMat = graphic_StackCount != null
                ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                : equipment.Graphic.MatSingle;

            // draw weapon
            Graphics.DrawMesh(mesh, drawLoc + drawingOffset, Quaternion.AngleAxis(turnAngle, Vector3.up), weaponMat, 0);

            // draws hands on equipment, if corresponding Comp is specified
            var compHandsDrawer = equipment.TryGetComp<CompHandsDrawer>();
            if (compHandsDrawer != null)
            {
                var handMat = GraphicDatabase.Get<Graphic_Single>("Overlays/Hand", ShaderDatabase.CutoutSkin, Vector2.one, Pawn.story.SkinColor).MatSingle;

                float offsetX, offsetY, offsetZ;
                if (compHandsDrawer.FirstHandPosition != Vector3.zero)
                {
                    offsetX = flipped
                        ? -compHandsDrawer.FirstHandPosition.x
                        : compHandsDrawer.FirstHandPosition.x;
                    offsetY = compHandsDrawer.FirstHandPosition.y;
                    offsetZ = compHandsDrawer.FirstHandPosition.z;

                    Graphics.DrawMesh(mesh,
                        drawLoc + drawingOffset + new Vector3(offsetX, offsetY, offsetZ).RotatedBy(turnAngle),
                        Quaternion.AngleAxis(turnAngle, Vector3.up), handMat, 0);
                }
                if (compHandsDrawer.SecondHandPosition != Vector3.zero)
                {
                    offsetX = flipped
                        ? -compHandsDrawer.SecondHandPosition.x
                        : compHandsDrawer.SecondHandPosition.x;
                    offsetY = compHandsDrawer.SecondHandPosition.y;
                    offsetZ = compHandsDrawer.SecondHandPosition.z;

                    Graphics.DrawMesh(mesh,
                        drawLoc + drawingOffset + new Vector3(offsetX, offsetY, offsetZ).RotatedBy(turnAngle),
                        Quaternion.AngleAxis(turnAngle, Vector3.up), handMat, 0);
                }
            }
        }
    }
}