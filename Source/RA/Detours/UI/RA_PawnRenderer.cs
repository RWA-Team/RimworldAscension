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

        public bool CarryWeaponOpenly
            =>
                (Pawn.carrier?.CarriedThing == null) &&
                (Pawn.Drafted || (Pawn.CurJob?.def.alwaysShowWeapon ?? false) ||
                 (Pawn.mindState.duty?.def.alwaysShowWeapon ?? false));

        public bool Aiming()
        {
            var stance_Busy = Pawn.stances.curStance as Stance_Busy;
            return stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid;
        }

        public Pawn Pawn => Initializer.GetHiddenValue(typeof (PawnRenderer), this, "pawn", infoPawn) as Pawn;

        public JitterHandler Jitterer
            => Initializer.GetHiddenValue(typeof (Pawn_DrawTracker), Pawn.Drawer, "jitterer", infoJitterer) as
                JitterHandler;

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
            if (Pawn.CurJob?.def.neverShowWeapon ?? false)
            {
                return;
            }
            var stance_Busy = Pawn.stances.curStance as Stance_Busy;
            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                var aimVector = stance_Busy.focusTarg.HasThing
                    ? stance_Busy.focusTarg.Thing.DrawPos
                    : stance_Busy.focusTarg.Cell.ToVector3Shifted();
                var num = 0f;
                if ((aimVector - Pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    num = (aimVector - Pawn.DrawPos).AngleFlat();
                }
                var drawLoc = rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
                drawLoc.y += 0.04f;
                // default weapon angle axis is upward, but all weapons are facing right, so we turn base weapon angle by 90°
                num -= 90f;
                DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc, num);
            }
            else if (CarryWeaponOpenly)
            {
                if (Pawn.Rotation == Rot4.South)
                {
                    var drawLoc2 = rootLoc + new Vector3(0f, 0f, -0.22f);
                    drawLoc2.y += 0.04f;
                    DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc2, 0f);
                }
                else if (Pawn.Rotation == Rot4.North)
                {
                    var drawLoc3 = rootLoc + new Vector3(0f, 0f, -0.11f);
                    DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc3, 0f);
                }
                else if (Pawn.Rotation == Rot4.East)
                {
                    var drawLoc4 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
                    drawLoc4.y += 0.04f;
                    DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc4, 0f);
                }
                else if (Pawn.Rotation == Rot4.West)
                {
                    var drawLoc5 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
                    drawLoc5.y += 0.04f;
                    DrawEquipmentAiming(Pawn.equipment.Primary, drawLoc5, 180f);
                }
            }
        }

        // draws hands on equipment and adjusts aiming angle position, if corresponding Comp is specified
        public new void DrawEquipmentAiming(Thing equipment, Vector3 weaponDrawLoc, float aimAngle)
        {
            var compWeaponExtensions = Pawn.equipment.Primary.TryGetComp<CompWeaponExtensions>();

            float weaponAngle;
            Vector3 weaponPositionOffset;

            Mesh weaponMesh;
            bool flipped;
            var aiming = Aiming();
            if (aimAngle > 110 && aimAngle < 250)
            {
                flipped = true;

                // flip weapon texture
                weaponMesh = MeshPool.GridPlaneFlip(equipment.Graphic.drawSize);

                weaponPositionOffset = compWeaponExtensions?.WeaponPositionOffset ?? Vector3.zero;
                // draw weapon beneath the pawn
                weaponPositionOffset += new Vector3(0, -0.5f, 0);

                weaponAngle = aimAngle - 180f;
                weaponAngle -= !aiming
                    ? equipment.def.equippedAngleOffset
                    : (compWeaponExtensions?.AttackAngleOffset ?? 0);
            }
            else
            {
                flipped = false;

                weaponMesh = MeshPool.GridPlane(equipment.Graphic.drawSize);

                weaponPositionOffset = -compWeaponExtensions?.WeaponPositionOffset ?? Vector3.zero;

                weaponAngle = aimAngle;
                weaponAngle += !aiming
                    ? equipment.def.equippedAngleOffset
                    : (compWeaponExtensions?.AttackAngleOffset ?? 0);
            }

            // weapon angle and position offsets based on current attack animation sequence
            DoAttackAnimationOffsets(ref weaponAngle, ref weaponPositionOffset, flipped);

            var graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            var weaponMat = graphic_StackCount != null
                ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                : equipment.Graphic.MatSingle;

            // draw weapon
            Graphics.DrawMesh(weaponMesh, weaponDrawLoc + weaponPositionOffset,
                Quaternion.AngleAxis(weaponAngle, Vector3.up),
                weaponMat, 0);

            // draw hands on equipment, if CompWeaponExtensions defines them
            if (compWeaponExtensions != null)
            {
                DrawHands(weaponAngle, weaponDrawLoc + weaponPositionOffset, compWeaponExtensions, flipped);
            }
        }

        public void DoAttackAnimationOffsets(ref float weaponAngle, ref Vector3 weaponPosition, bool flipped)
        {
            var damageDef = Pawn.VerbTracker?.PrimaryVerb?.verbProps?.meleeDamageDef;
            if (damageDef != null)
            {
                // total weapon angle change during animation sequence
                var totalSwingAngle = 0;
                var animationPhasePercent = Jitterer.CurrentJitterOffset.magnitude/Jitterer.JitterMax;
                if (damageDef == DamageDefOf.Stab)
                {
                    totalSwingAngle = 180;
                    weaponPosition += Jitterer.CurrentJitterOffset +
                                      new Vector3(0, 0, Mathf.Pow(Jitterer.CurrentJitterOffset.magnitude, 0.5f));
                }
                else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
                {
                    totalSwingAngle = 45;
                    weaponPosition += Jitterer.CurrentJitterOffset +
                                      new Vector3(0, 0,
                                          Mathf.Sin(Jitterer.CurrentJitterOffset.magnitude*Mathf.PI/Jitterer.JitterMax)/
                                          10);
                }
                weaponAngle += flipped
                    ? -animationPhasePercent * totalSwingAngle
                    : animationPhasePercent * totalSwingAngle;
            }
        }

        public void DrawHands(float weaponAngle, Vector3 weaponPosition, CompWeaponExtensions compWeaponExtensions,
            bool flipped)
        {
            var handMat =
                GraphicDatabase.Get<Graphic_Single>("Overlays/Hand", ShaderDatabase.CutoutSkin, Vector2.one,
                    Pawn.story.SkinColor).MatSingle;

            var handsMesh = MeshPool.GridPlane(Vector2.one);

            if (compWeaponExtensions.FirstHandPosition != Vector3.zero)
            {
                var handPosition = compWeaponExtensions.FirstHandPosition;
                if (flipped)
                {
                    handPosition = -handPosition;
                    // keep z the same
                    handPosition.z = -handPosition.z;
                }

                Graphics.DrawMesh(handsMesh,
                    weaponPosition + handPosition.RotatedBy(weaponAngle),
                    Quaternion.AngleAxis(weaponAngle, Vector3.up), handMat, 0);
            }
            if (compWeaponExtensions.SecondHandPosition != Vector3.zero)
            {
                var handPosition = compWeaponExtensions.SecondHandPosition;
                if (flipped)
                {
                    handPosition = -handPosition;
                    // keep z the same
                    handPosition.z = -handPosition.z;
                }

                Graphics.DrawMesh(handsMesh,
                    weaponPosition + handPosition.RotatedBy(weaponAngle),
                    Quaternion.AngleAxis(weaponAngle, Vector3.up), handMat, 0);
            }
        }
    }
}