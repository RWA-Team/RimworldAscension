using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace RA
{
    public class Target : Dummy
    {
        public int interactionCellOffset = 1;
        public static int interactionCellOffset_Transfer;

        public override void FindTrainee()
        {
            foreach (Pawn pawn in allowedPawns)
            {
                // find pawn with equipped ranged weapon and it's posible to hit target from interaction cell
                if (pawn.equipment.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_LaunchProjectile && pawn.equipment.PrimaryEq.PrimaryVerb.CanHitTargetFrom(InteractionCell, this))
                    // which is also idle, has no life needs and can reserve and reach
                    if (pawn.mindState.IsIdle && !PawnHasNeeds(pawn) && ReservationUtility.CanReserveAndReach(pawn, this, PathEndMode.InteractionCell, Danger.Some))
                    {
                        pawn.drafter.TakeOrderedJob(new Job(DefDatabase<JobDef>.GetNamed("CombatTraining"), this, this.InteractionCell));
                        break;
                    }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            // Increase Range gizmo
            yield return new Command_Action
            {
                defaultDesc = "Increase interaction cell offset",
                defaultLabel = "Increase Range",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade", true),
                activateSound = SoundDef.Named("Click"),
                action = new Action(IncreaseRange),
            };

            // Decrease Range gizmo
            yield return new Command_Action
            {
                defaultDesc = "Decrease interaction cell offset",
                defaultLabel = "Decrease Range",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade", true),
                activateSound = SoundDef.Named("Click"),
                action = new Action(DecreaseRange),
            };
        }

        public override void CopySettings()
        {
            base.CopySettings();

            Target.interactionCellOffset_Transfer = interactionCellOffset;
        }

        public override void PasteSettings()
        {
            base.PasteSettings();

            if (Target.interactionCellOffset_Transfer != 0)
                interactionCellOffset = Target.interactionCellOffset_Transfer;
        }

        public void IncreaseRange()
        {
            if (interactionCellOffset < 10)
                interactionCellOffset += 1;
        }

        public void DecreaseRange()
        {
            if (interactionCellOffset > 0)
                interactionCellOffset -= 1;
        }

        public override void DrawExtraSelectionOverlays()
        {
            Vector3 position = InteractionCell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, MaterialPool.MatFrom("UI/Overlays/InteractionCell", ShaderDatabase.Transparent), 0);
        }

        public override IntVec3 InteractionCell
        {
            get
            {
                // try to offset interaction cell until it's standable
                IntVec3 newInteractionCell;
                for (int i = interactionCellOffset; i > 0; i--)
                {
                    if (this.Rotation == Rot4.North)
                        newInteractionCell = base.InteractionCell + new IntVec3(0, 0, 0 - interactionCellOffset);
                    else if (this.Rotation == Rot4.East)
                        newInteractionCell = base.InteractionCell + new IntVec3(0 - interactionCellOffset, 0, 0);
                    else if (this.Rotation == Rot4.South)
                        newInteractionCell = base.InteractionCell + new IntVec3(0, 0, 0 + interactionCellOffset);
                    else
                        newInteractionCell = base.InteractionCell + new IntVec3(0 + interactionCellOffset, 0, 0);

                    if (newInteractionCell.Standable())
                        return newInteractionCell;
                }

                // otherwise return base value
                return base.InteractionCell;
            }
        }

        public override string GetInspectString()
        {
            Pawn pawn = Find.Reservations.FirstReserverOf(this, Faction);
            StringBuilder inspectString = new StringBuilder();
            if (pawn != null && pawn.equipment.Primary != null && pawn.Position == InteractionCell)
            {
                Verb_LaunchProjectile attackVerb = pawn.equipment.PrimaryEq.PrimaryVerb as Verb_LaunchProjectile;

                if (attackVerb != null && attackVerb.CanHitTarget(this))
                {
                    HitReport hitReport = attackVerb.HitReportFor(this);
                    inspectString.AppendFormat("{0} shooting skill:\t\t\t{1} ({2})\n", pawn.NameStringShort, pawn.skills.GetSkill(SkillDefOf.Shooting).LevelDescriptor, pawn.skills.GetSkill(SkillDefOf.Shooting).level);
                    inspectString.AppendLine("Total hit chance at this range:\t\t" + GenText.AsPercent(hitReport.TotalHitChance));
                    inspectString.AppendLine("Pawn stats accuracy modifier:\t\t" + GenText.AsPercent(hitReport.hitChanceThroughPawnStat));
                    inspectString.AppendLine("Weapon stats accuracy modifier:\t" + GenText.AsPercent(hitReport.hitChanceThroughEquipment));
                    inspectString.AppendLine("DPS with current accuracy:\t\t" + CalculateShootingDPS(hitReport, pawn.equipment.Primary.def).ToString("F1"));
                    if (pawn.skills.GetSkill(SkillDefOf.Shooting).level < 10)
                    {
                        inspectString.AppendLine("ExpPS with current weapon:\t\t" + (pawn.skills.GetSkill(SkillDefOf.Shooting).LearningFactor * 10f / pawn.GetStatValue(StatDefOf.RangedWeapon_Cooldown)).ToString("F1"));
                    }
                    else
                    {
                        inspectString.AppendLine("Pawn needs real combat to progress further.");
                    }
                }
                else
                {
                    inspectString.AppendLine(pawn.NameStringShort + " cannot hit target");
                }
            }
            return inspectString.ToString();
        }

        public float CalculateShootingDPS(HitReport hitReport, ThingDef weaponDef)
        {
            float projectileDamage = weaponDef.Verbs[0].projectileDef == null ? 0 : weaponDef.Verbs[0].projectileDef.projectile.damageAmountBase;
            float cooldown = weaponDef.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown);
            float burstCount = weaponDef.Verbs[0].burstShotCount;
            float burstShotsInterval = (float)weaponDef.Verbs[0].ticksBetweenBurstShots / GenTicks.TicksPerRealtimeSecond;

            return projectileDamage * burstCount * hitReport.TotalHitChance / (cooldown + (burstCount - 1) * burstShotsInterval);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<int>(ref interactionCellOffset, "interactionCellOffset");
        }
    }
}