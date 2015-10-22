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
    public class Building_TargetDummy : Building_Dummy
    {
        public int interactionCellOffset = 1;
        public static int interactionCellOffset_Transfer;

        public override void FindTrainee()
        {
            foreach (Pawn pawn in allowedPawns)
            {
                // find pawn with equipped ranged weapon
                if (pawn.equipment.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_LaunchProjectile)
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

            Command_Action gizmo_increaseRange = new Command_Action
            {
                defaultDesc = "Increase interaction cell offset",
                defaultLabel = "Increase Range",
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/Upgrade", true),
                activateSound = SoundDef.Named("Click"),
                action = new Action(IncreaseRange),
            };
            yield return gizmo_increaseRange;

            Command_Action gizmo_decreaseRange = new Command_Action
            {
                defaultDesc = "Decrease interaction cell offset",
                defaultLabel = "Decrease Range",
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/Upgrade", true),
                activateSound = SoundDef.Named("Click"),
                action = new Action(DecreaseRange),
            };
            yield return gizmo_decreaseRange;
        }

        public override void CopySettings()
        {
            base.CopySettings();

            Building_TargetDummy.interactionCellOffset_Transfer = interactionCellOffset;
        }

        public override void PasteSettings()
        {
            base.PasteSettings();

            if (Building_TargetDummy.interactionCellOffset_Transfer != 0)
                interactionCellOffset = Building_TargetDummy.interactionCellOffset_Transfer;
        }

        public void IncreaseRange()
        {
            if (interactionCellOffset < 10)
                interactionCellOffset += 1;
        }

        // minimum range is 2 cells
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
                        newInteractionCell = base.InteractionCell + new IntVec3(0, 0, 0 + interactionCellOffset);
                    else if (this.Rotation == Rot4.East)
                        newInteractionCell = base.InteractionCell + new IntVec3(0 + interactionCellOffset, 0, 0);
                    else if (this.Rotation == Rot4.South)
                        newInteractionCell = base.InteractionCell + new IntVec3(0, 0, 0 - interactionCellOffset);
                    else
                        newInteractionCell = base.InteractionCell + new IntVec3(0 - interactionCellOffset, 0, 0);

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
                    inspectString.AppendLine(pawn.NameStringShort + " shooting skill level:\t" + pawn.skills.GetSkill(SkillDefOf.Shooting).LevelDescriptor);
                    inspectString.AppendLine("Total hit chance at this range:\t" + GenText.AsPercent(hitReport.TotalHitChance));
                    inspectString.AppendLine("Pawn stats accuracy modifier:\t" + GenText.AsPercent(hitReport.hitChanceThroughPawnStat));
                    inspectString.AppendLine("Weapon stats accuracy modifier:\t" + GenText.AsPercent(hitReport.hitChanceThroughEquipment));
                    //inspectString.AppendLine("Current DPS:\t" + CalculateDPS(hitReport, pawn, pawn.equipment.Primary.def));
                }
                else
                {
                    inspectString.AppendLine(pawn.NameStringShort + " cannot hit target");
                }
            }
            return inspectString.ToString();
        }
        /*
        public string CalculateDPS(HitReport hitReport, Pawn pawn, ThingDef weaponDef)
        {
            //hitReport.TotalHitChance
            int damageGetter = weaponDef.Verbs[0].projectileDef == null ? 0 : weaponDef.Verbs[0].projectileDef.projectile.damageAmountBase;
            float warmupGetter = weaponDef.Verbs[0].warmupTicks / 60f;
            float cooldownGetter = weaponDef.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown, null);
            int burstShotsGetter = weaponDef.Verbs[0].burstShotCount;
            float dpsRawGetter = delegate(ThingDef d)
            {
                int num = burstShotsGetter(d);
                float num2 = warmupGetter(d) + cooldownGetter(d);
                num2 += (float)(num - 1) * ((float)d.Verbs[0].ticksBetweenBurstShots / 60f);
                return (float)(damageGetter(d) * num) / num2;
            };
        }*/

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<int>(ref interactionCellOffset, "interactionCellOffset", 0);
        }
    }
}