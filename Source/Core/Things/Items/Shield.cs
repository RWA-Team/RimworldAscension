using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace RA
{
    class Shield : Apparel
    {
        public static readonly SoundDef SoundAbsorbDamage = SoundDef.Named("PersonalShieldAbsorbDamage");
        public static readonly SoundDef SoundBreak = SoundDef.Named("PersonalShieldBroken");

        public const int BaseAbsorbChance_Melee = 25;
        public const int BaseAbsorbChance_Ranged = 50;

        // determine when to display shield texture
        public bool ShouldDisplay
        {
            get
            {
                return !this.wearer.Dead && !this.wearer.Downed && (!this.wearer.IsPrisonerOfColony || (this.wearer.BrokenStateDef != null && this.wearer.BrokenStateDef.isAggro)) && (this.wearer.Drafted || this.wearer.Faction.HostileTo(Faction.OfColony));
            }
        }

        // absorbs recieved damage
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (dinfo.Instigator != null)
            {
                SkillRecord meleeSkill = this.wearer.skills.GetSkill(SkillDefOf.Melee);
                float hitRoll = new System.Random().Next(1, 100);

                Log.Message("roll " + hitRoll);
                Log.Message("source melee? " + dinfo.Instigator.Position.AdjacentTo8WayOrInside(this.wearer.Position));

                // instigator is melee and damage not explosive
                if (dinfo.Instigator.Position.AdjacentTo8WayOrInside(this.wearer.Position) && !dinfo.Def.isExplosive)
                {
                    if (hitRoll <= BaseAbsorbChance_Melee + meleeSkill.level * 2)
                    {
                        return TryAbsorbDamage(dinfo);
                    }
                }

                // instigator is ranged and damage not explosive
                if (!dinfo.Instigator.Position.AdjacentTo8WayOrInside(this.wearer.Position) && !dinfo.Def.isExplosive)
                {
                    if (hitRoll <= BaseAbsorbChance_Ranged + meleeSkill.level * 2)
                    {
                        return TryAbsorbDamage(dinfo);
                    }
                }
            }

            return false;
        }

        // returns true if damage is completely absorbed
        public bool TryAbsorbDamage(DamageInfo dinfo)
        {
            this.HitPoints -= dinfo.Amount;

            if (this.HitPoints <= 0)
            {
                Log.Message("base dmg " + dinfo.Amount);
                dinfo.SetAmount(dinfo.Amount + this.HitPoints);
                Log.Message("after dmg " + dinfo.Amount);
                Break();
                return false;
            }
            else
            {
                ThrowAbsorbationEffects(dinfo);
                return true;
            }
        }

        // throws special effects when damage is absorved
        public void ThrowAbsorbationEffects(DamageInfo dinfo)
        {
            SoundAbsorbDamage.PlayOneShot(this.wearer.Position);
            Vector3 loc = this.wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle).RotatedBy(180f) * 0.5f;
            MoteThrower.ThrowStatic(loc, ThingDefOf.Mote_ShotHit_Spark, 1f);
        }

        public void Break()
        {
            SoundBreak.PlayOneShot(this.wearer.Position);
            this.Destroy();
        }

        // allows to attack only adjacent cells (melee targets)
        public override bool AllowVerbCast(IntVec3 root, TargetInfo targ)
        {
            if (targ.HasThing && targ.Thing.def.size != IntVec2.One)
            {
                return root.AdjacentTo8WayOrInside(targ.Thing);
            }
            return root.AdjacentTo8Way(targ.Cell);
        }

        // draws thing on pawn
        public override void DrawWornExtras()
        {
            float turnAngle = 0f;
            Vector3 drawCenter = this.wearer.drawer.DrawPos;
            drawCenter.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
            Vector3 s = new Vector3(1f, 1f, 1f);
            if (this.wearer.Rotation == Rot4.North)
            {
                drawCenter.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
                drawCenter.x -= 0.2f;
                drawCenter.z -= 0.2f;
            }

            if (this.wearer.Rotation == Rot4.South)
            {
                drawCenter.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                drawCenter.x += 0.2f;
                drawCenter.z -= 0.2f;
            }

            if (this.wearer.Rotation == Rot4.East)
            {
                drawCenter.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
                drawCenter.z -= 0.2f;
                turnAngle = 90f;
            }

            if (this.wearer.Rotation == Rot4.West)
            {
                drawCenter.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                drawCenter.z -= 0.2f;
                turnAngle = 300f;
            }

            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawCenter, Quaternion.AngleAxis(turnAngle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.Graphic.MatSingle, 0);
        }

        // draws gizmo with shield durability
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            base.GetWornGizmos();

            Gizmo_ShieldStatus gizmoShield = new Gizmo_ShieldStatus
            {
                shield = this                
            };
            yield return gizmoShield;
        }
    }

    internal class Gizmo_ShieldStatus : Gizmo
    {
        public static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        public static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Shield shield;

        public override float Width
        {
            get
            {
                return Height;
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.Width, Height);
            Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, shield.LabelCap);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = shield.HitPoints / shield.MaxHitPoints;
                Widgets.FillableBar(rect3, fillPercent, FullTex, EmptyTex, false);

                Widgets.ThingIcon(rect, shield);

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, shield.HitPoints + " / " + shield.MaxHitPoints);
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
