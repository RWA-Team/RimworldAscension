using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public class Target : Dummy
    {
        public int interactionCellOffset = 1;
        public static int interactionCellOffset_Transfer;

        public override void TryAssignTraining()
        {
            foreach (var pawn in allowedPawns)
            {
                // find pawn with equipped ranged weapon and it's posible to hit target from interaction cell
                if (pawn.equipment.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_LaunchProjectile && pawn.equipment.PrimaryEq.PrimaryVerb.CanHitTargetFrom(InteractionCell, this))
                    // which is also idle, has no life needs and can reserve and reach
                    if (pawn.mindState.IsIdle && !PawnHasNeeds(pawn) && pawn.CanReserveAndReach(this, PathEndMode.InteractionCell, Danger.Some))
                    {
                        pawn.drafter.TakeOrderedJob(new Job(DefDatabase<JobDef>.GetNamed("CombatTraining"), this, InteractionCell));
                        break;
                    }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            // Increase Range gizmo
            yield return new Command_Action
            {
                defaultDesc = "Increase interaction cell offset",
                defaultLabel = "Increase Range",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade"),
                activateSound = SoundDef.Named("Click"),
                action = IncreaseRange
            };

            // Decrease Range gizmo
            yield return new Command_Action
            {
                defaultDesc = "Decrease interaction cell offset",
                defaultLabel = "Decrease Range",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade"),
                activateSound = SoundDef.Named("Click"),
                action = DecreaseRange
            };
        }

        public override void CopySettings()
        {
            base.CopySettings();

            interactionCellOffset_Transfer = interactionCellOffset;
        }

        public override void PasteSettings()
        {
            base.PasteSettings();

            if (interactionCellOffset_Transfer != 0)
                interactionCellOffset = interactionCellOffset_Transfer;
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
            var position = InteractionCell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, MaterialPool.MatFrom("UI/Overlays/InteractionCell", ShaderDatabase.Transparent), 0);
        }

        public override IntVec3 InteractionCell
        {
            get
            {
                // try to offset interaction cell until it's standable
                for (var i = interactionCellOffset; i > 0; i--)
                {
                    IntVec3 newInteractionCell;
                    if (Rotation == Rot4.North)
                        newInteractionCell = base.InteractionCell + new IntVec3(0, 0, 0 - interactionCellOffset);
                    else if (Rotation == Rot4.East)
                        newInteractionCell = base.InteractionCell + new IntVec3(0 - interactionCellOffset, 0, 0);
                    else if (Rotation == Rot4.South)
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
            var pawn = Find.Reservations.FirstReserverOf(this, Faction);
            var inspectString = new StringBuilder();
            if (pawn?.equipment.Primary != null && pawn.Position == InteractionCell)
            {
                var attackVerb = pawn.equipment.PrimaryEq.PrimaryVerb as Verb_LaunchProjectile;

                if (attackVerb != null && attackVerb.CanHitTarget(this))
                {
                    var hitReport = attackVerb.HitReportFor(this);
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
            float projectileDamage = weaponDef.Verbs[0].projectileDef?.projectile.damageAmountBase ?? 0;
            var cooldown = weaponDef.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown);
            float burstCount = weaponDef.Verbs[0].burstShotCount;
            var burstShotsInterval = weaponDef.Verbs[0].ticksBetweenBurstShots / GenDate.SecondsToTicks(1);

            return projectileDamage * burstCount * hitReport.TotalHitChance / (cooldown + (burstCount - 1) * burstShotsInterval);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref interactionCellOffset, "interactionCellOffset");
        }
    }
}