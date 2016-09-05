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

        public Pawn Pawn => Initializer.GetHiddenValue(typeof (PawnRenderer), this, "pawn", infoPawn) as Pawn;

        public JitterHandler Jitterer
            => Initializer.GetHiddenValue(typeof (Pawn_DrawTracker), Pawn.Drawer, "jitterer", infoJitterer) as
                    JitterHandler;

        // draws hands on equipment and adjusts aiming angle position, if corresponding Comp is specified
        public new void DrawEquipmentAiming(Thing equipment, Vector3 weaponDrawLoc, float aimAngle)
        {
            var compWeaponExtensions = Pawn.equipment.Primary.TryGetComp<CompWeaponExtensions>();

            float weaponAngle = 0;
            if (!Aiming())
            {
                // resets vanilla carry weapon angle to 0°
                if (Pawn.Rotation == Rot4.West || Pawn.Rotation == Rot4.North)
                    aimAngle = 270;
            }
            else
            {
                // default weapon angle axis is upward, but all weapons are facing right, so we turn base weapon angle by 90°
                weaponAngle = aimAngle - 90;
                weaponAngle += compWeaponExtensions?.AttackAngleOffset ?? 0;
            }
            
            var weaponPositionOffset = compWeaponExtensions?.WeaponPositionOffset ?? Vector3.zero;

            // weapon angle and position offsets based on current attack animation sequence
            AttackAnimationOffsets(ref weaponAngle, ref weaponPositionOffset, compWeaponExtensions);

            Mesh weaponMesh;
            var flipped = false;
            if (aimAngle > 200 && aimAngle< 340)
            {
                flipped = true;
                // flip weapon texture
                weaponMesh = MeshPool.GridPlaneFlip(equipment.Graphic.drawSize);
                // draw weapon beneath the pawn
                weaponPositionOffset += new Vector3(0, -0.5f, 0);
            }
            else
            {
                weaponMesh = MeshPool.GridPlane(equipment.Graphic.drawSize);
            }

            // adjust flipped weapon rotation
            if (Aiming())
            {
                weaponAngle += flipped
                    ? compWeaponExtensions?.AttackAngleOffset ?? 0
                    : -compWeaponExtensions?.AttackAngleOffset ?? 0;
            }
            else
            {
                weaponAngle += flipped
                    ? equipment.def.equippedAngleOffset
                    : -equipment.def.equippedAngleOffset;
            }

            var graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            var weaponMat = graphic_StackCount != null
                ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                : equipment.Graphic.MatSingle;

            // draw weapon
            Graphics.DrawMesh(weaponMesh, weaponDrawLoc + weaponPositionOffset, Quaternion.AngleAxis(weaponAngle, Vector3.up),
                weaponMat, 0);

            // draw hands on equipment, if CompWeaponExtensions defines them
            if (compWeaponExtensions != null)
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
                        handPosition.z = -handPosition.z;
                    }
                    
                    Graphics.DrawMesh(handsMesh,
                        weaponDrawLoc + weaponPositionOffset + handPosition.RotatedBy(weaponAngle),
                        Quaternion.AngleAxis(weaponAngle, Vector3.up), handMat, 0);
                }
                if (compWeaponExtensions.SecondHandPosition != Vector3.zero)
                {
                    var handPosition = compWeaponExtensions.SecondHandPosition;
                    if (flipped)
                    {
                        handPosition = -handPosition;
                        handPosition.z = -handPosition.z;
                    }

                    Graphics.DrawMesh(handsMesh,
                        weaponDrawLoc + weaponPositionOffset + handPosition.RotatedBy(weaponAngle),
                        Quaternion.AngleAxis(weaponAngle, Vector3.up), handMat, 0);
                }
            }
        }

        public void AttackAnimationOffsets(ref float weaponAngle, ref Vector3 weaponPosition, CompWeaponExtensions compWeaponExtensions = null)
        {
            var damageDef = Pawn.VerbTracker?.PrimaryVerb?.verbProps?.meleeDamageDef;
            if (damageDef != null)
            {
                if (damageDef == DamageDefOf.Stab)
                {
                    // total angle weapon changes during animation sequence
                    var swingAngle = Mathf.Abs(compWeaponExtensions?.AttackAngleOffset ?? 45);
                    weaponAngle -= Jitterer.CurrentJitterOffset.magnitude/Jitterer.JitterMax*
                                                swingAngle;

                    weaponPosition += Jitterer.CurrentJitterOffset +
                                      new Vector3(0, 0, Mathf.Pow(Jitterer.CurrentJitterOffset.magnitude, 0.5f));
                        //Jitterer.CurrentJitterOffset.magnitude * weaponRotationPerTick;
                }
                else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
                {
                    var swingAngle = Mathf.Abs(compWeaponExtensions?.AttackAngleOffset ?? 135);
                    weaponAngle -= Jitterer.CurrentJitterOffset.magnitude/Jitterer.JitterMax*swingAngle;

                    weaponPosition += Jitterer.CurrentJitterOffset +
                                      new Vector3(0, 0,
                                          Mathf.Sin(Jitterer.CurrentJitterOffset.magnitude*Mathf.PI/Jitterer.JitterMax)/
                                          10);
                    //Jitterer.CurrentJitterOffset.magnitude * weaponRotationPerTick;
                }
            }
        }
    }
}