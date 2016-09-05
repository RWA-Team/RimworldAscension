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
            var weaponAngleOffset = 0f;
            var weaponPositionOffset = new Vector3();

            // resets vanilla weapon angle to 0
            if (!Aiming())
            {
                aimAngle = 0;
            }

            // weapon angle offset when facing north and west
            if (Pawn.Rotation == Rot4.West || Pawn.Rotation == Rot4.North)
            {
                weaponPositionOffset += new Vector3(0, -0.5f, 0);
                weaponAngleOffset += equipment.def.equippedAngleOffset;
            }
            else
            {
                // weapon angle offset based on <equippedAngleOffset> param
                weaponAngleOffset -= equipment.def.equippedAngleOffset;
            }


            // weapon angle and position offsets based on CompWeaponExtensions params
            var compWeaponExtensions = Pawn.equipment.Primary.TryGetComp<CompWeaponExtensions>();
            if (compWeaponExtensions != null)
            {
                weaponPositionOffset += compWeaponExtensions.WeaponPositionOffset;
                if (Aiming()) weaponAngleOffset += compWeaponExtensions.AttackAngleOffset;
            }

            AttackAnimationOffsets(ref weaponAngleOffset, ref weaponPositionOffset, compWeaponExtensions);

            var turnAngle = aimAngle;
            var flipped = false;
            Mesh weaponMesh;
            Log.Message("aimAngle " + aimAngle);
            Log.Message("turnAngle " + turnAngle);
            Log.Message("weaponAngleOffset " + weaponAngleOffset);
            if (aimAngle > 20f && aimAngle < 160f)
            {
                weaponMesh = MeshPool.GridPlane(equipment.Graphic.drawSize);
                turnAngle += weaponAngleOffset;
                Log.Message("turnAngle result: " + turnAngle);
                Log.Message("**********************");
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                weaponMesh = MeshPool.GridPlaneFlip(equipment.Graphic.drawSize);
                //turnAngle -= 180f;
                turnAngle -= weaponAngleOffset;
                flipped = true;
            }
            else
            {
                weaponMesh = MeshPool.GridPlane(equipment.Graphic.drawSize);
                turnAngle += weaponAngleOffset;
            }
            turnAngle %= 360f;

            var graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            var weaponMat = graphic_StackCount != null
                ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                : equipment.Graphic.MatSingle;

            // draw weapon
            Graphics.DrawMesh(weaponMesh, weaponDrawLoc + weaponPositionOffset, Quaternion.AngleAxis(turnAngle, Vector3.up),
                weaponMat, 0);

            // draw hands on equipment, if CompWeaponExtensions defines them
            if (compWeaponExtensions != null)
            {
                var handMat =
                    GraphicDatabase.Get<Graphic_Single>("Overlays/Hand", ShaderDatabase.CutoutSkin, Vector2.one,
                        Pawn.story.SkinColor).MatSingle;

                var handsMesh = MeshPool.plane10;

                if (compWeaponExtensions.FirstHandPosition != Vector3.zero)
                {
                    var handPosition = compWeaponExtensions.FirstHandPosition;
                    if (flipped) handPosition = -handPosition;

                    Graphics.DrawMesh(handsMesh,
                        weaponDrawLoc + weaponPositionOffset + handPosition.RotatedBy(turnAngle),
                        Quaternion.AngleAxis(turnAngle, Vector3.up), handMat, 0);
                }
                if (compWeaponExtensions.SecondHandPosition != Vector3.zero)
                {
                    var handPosition = compWeaponExtensions.FirstHandPosition;
                    if (flipped) handPosition = -handPosition;

                    Graphics.DrawMesh(handsMesh,
                        weaponDrawLoc + weaponPositionOffset + handPosition.RotatedBy(turnAngle),
                        Quaternion.AngleAxis(turnAngle, Vector3.up), handMat, 0);
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
                    var swingAngle = Mathf.Abs(compWeaponExtensions.AttackAngleOffset - 90);
                    weaponAngle += Jitterer.CurrentJitterOffset.magnitude/Jitterer.JitterMax*
                                                swingAngle;

                    weaponPosition += Jitterer.CurrentJitterOffset +
                                      new Vector3(0, 0, Mathf.Pow(Jitterer.CurrentJitterOffset.magnitude, 0.5f));
                        //Jitterer.CurrentJitterOffset.magnitude * weaponRotationPerTick;
                }
                else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
                {
                    var swingAngle = Mathf.Abs(Pawn.equipment.Primary.def.equippedAngleOffset + compWeaponExtensions.AttackAngleOffset - 90);
                    weaponAngle += Jitterer.CurrentJitterOffset.magnitude/Jitterer.JitterMax*swingAngle;

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