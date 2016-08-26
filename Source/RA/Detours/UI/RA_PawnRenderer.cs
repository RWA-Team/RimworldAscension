using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_PawnRenderer : PawnRenderer
    {
        public static FieldInfo infoPawn;
        public static FieldInfo infoJitterer;

        public RA_PawnRenderer(Pawn Pawn) : base(Pawn)
        {
        }

        public bool Aiming()
        {
            var stance_Busy = Pawn.stances.curStance as Stance_Busy;
            return stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid;
        }

        public Pawn Pawn => Initializer.GetHiddenValue(typeof(PawnRenderer), this, "pawn", infoPawn) as Pawn;
        public JitterHandler Jitterer => Initializer.GetHiddenValue(typeof(Pawn_DrawTracker), Pawn.Drawer, "jitterer", infoJitterer) as JitterHandler;

        // draws hands on equipment and adjusts aiming angle position, if corresponding Comp is specified
        public new void DrawEquipmentAiming(Thing equipment, Vector3 drawLoc, float aimAngle)
        {
            var angleOffset = equipment.def.equippedAngleOffset;

            var aiming = Aiming();

            var CompWeaponExtensions = Pawn.equipment.Primary.TryGetComp<CompWeaponExtensions>();
            // adjusts aiming angle position
            if (CompWeaponExtensions != null && aiming) angleOffset += CompWeaponExtensions.AimingAngleOffset;

            // used to draw weapon beneath the Pawn when facing north and west
            var weaponPositionOffset = Pawn.Rotation == Rot4.West || Pawn.Rotation == Rot4.North
                ? new Vector3(0, -0.5f, 0)
                : Vector3.zero;

            // used to offset weapon drawing position based on current attack animation state
            weaponPositionOffset += AttackAnimationOffset();

            // used to offset weapon drawing position
            weaponPositionOffset += CompWeaponExtensions?.WeaponPositionOffset ?? Vector3.zero;

            var turnAngle = aimAngle - 90f;
            var flipped = false;

            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                turnAngle += angleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                turnAngle -= 180f;
                turnAngle -= angleOffset;
                flipped = true;
            }
            else
            {
                mesh = MeshPool.plane10;
                turnAngle += angleOffset;
            }
            turnAngle %= 360f;

            var graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            var weaponMat = graphic_StackCount != null
                ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                : equipment.Graphic.MatSingle;

            // draw weapon
            Graphics.DrawMesh(mesh, drawLoc + weaponPositionOffset, Quaternion.AngleAxis(turnAngle, Vector3.up), weaponMat, 0);

            // draws hands on equipment, if corresponding Comp is specified
            if (CompWeaponExtensions != null)
            {
                var handMat = GraphicDatabase.Get<Graphic_Single>("Overlays/Hand", ShaderDatabase.CutoutSkin, Vector2.one, Pawn.story.SkinColor).MatSingle;

                float offsetX, offsetY, offsetZ;
                if (CompWeaponExtensions.FirstHandPosition != Vector3.zero)
                {
                    offsetX = flipped
                        ? -CompWeaponExtensions.FirstHandPosition.x
                        : CompWeaponExtensions.FirstHandPosition.x;
                    offsetY = CompWeaponExtensions.FirstHandPosition.y;
                    offsetZ = CompWeaponExtensions.FirstHandPosition.z;

                    Graphics.DrawMesh(mesh,
                        drawLoc + weaponPositionOffset + new Vector3(offsetX, offsetY, offsetZ).RotatedBy(turnAngle),
                        Quaternion.AngleAxis(turnAngle, Vector3.up), handMat, 0);
                }
                if (CompWeaponExtensions.SecondHandPosition != Vector3.zero)
                {
                    offsetX = flipped
                        ? -CompWeaponExtensions.SecondHandPosition.x
                        : CompWeaponExtensions.SecondHandPosition.x;
                    offsetY = CompWeaponExtensions.SecondHandPosition.y;
                    offsetZ = CompWeaponExtensions.SecondHandPosition.z;

                    Graphics.DrawMesh(mesh,
                        drawLoc + weaponPositionOffset + new Vector3(offsetX, offsetY, offsetZ).RotatedBy(turnAngle),
                        Quaternion.AngleAxis(turnAngle, Vector3.up), handMat, 0);
                }
            }
        }

        public Vector3 AttackAnimationOffset()
        {
            var curAttackOffset = Vector3.zero;
            var damageDef = Pawn.VerbTracker.PrimaryVerb.verbProps.meleeDamageDef;
            if (damageDef != null)
            {
                if (damageDef == DamageDefOf.Stab)
                {
                    curAttackOffset = Jitterer.CurrentJitterOffset;
                }
            }

            return curAttackOffset;
        }
    }
}