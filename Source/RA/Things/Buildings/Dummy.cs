using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public class Dummy : Building
    {
        public List<Pawn> allowedPawns = new List<Pawn>();
        public static List<Pawn> allowedPawns_Transfer;

        public virtual void TryAssignTraining()
        {
            foreach (var pawn in allowedPawns)
            {
                // find pawn with equipped melee weapon
                if (pawn.equipment.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_MeleeAttack)
                    // which is also idle, has no life needs and can reserve and reach
                    if (pawn.mindState.IsIdle && !PawnHasNeeds(pawn) && pawn.CanReserveAndReach(this, PathEndMode.InteractionCell, Danger.Some))
                    {
                        pawn.drafter.TakeOrderedJob(new Job(DefDatabase<JobDef>.GetNamed("CombatTraining"), this, InteractionCell));
                        break;
                    }
            }
        }

        // checks if pawn has any life needs below optimal
        public bool PawnHasNeeds(Pawn pawn)
        {
            return pawn.needs.food.CurCategory >= HungerCategory.Hungry || pawn.needs.rest.CurCategory >= RestCategory.Tired || pawn.needs.joy.CurCategory <= JoyCategory.Low;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            if (allowedPawns.Contains(pawn))
            {
                yield return new FloatMenuOption(pawn.Name + " already assigned to the trainees list", null);
            }
            else
            {
                Action action = () =>
                {
                    allowedPawns.Add(pawn);
                };
                yield return new FloatMenuOption("Assign " + pawn.Name + " to trainees list", action);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            var gizmo_copySettings = new Command_Action
            {
                defaultDesc = "Copy dummy's settings",
                defaultLabel = "Copy Settings",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings"),
                activateSound = SoundDef.Named("Click"),
                action = CopySettings
            };
            yield return gizmo_copySettings;

            var gizmo_pasteSettings = new Command_Action
            {
                defaultDesc = "Paste dummy's settings",
                defaultLabel = "Paste Settings",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings"),
                activateSound = SoundDef.Named("Click"),
                action = PasteSettings
            };
            yield return gizmo_pasteSettings;
        }

        public virtual void CopySettings()
        {
            allowedPawns_Transfer = new List<Pawn>(allowedPawns);
        }

        public virtual void PasteSettings()
        {
            if (allowedPawns_Transfer != null)
                allowedPawns = new List<Pawn>(allowedPawns_Transfer);
        }

        // do action each time dummy takes damage
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            var pawn = Find.Reservations.FirstReserverOf(this, Faction);

            // if zoom is close enough and dummy is selected
            if (Find.CameraMap.CurrentZoom == CameraZoomRange.Closest && (Find.Selector.IsSelected(this) || Find.Selector.IsSelected(pawn)))
            {
                // throws text mote with applied damage each time damage taken
                MoteThrower.ThrowText(new Vector3(Position.x + 0.5f, Position.y, Position.z + 1f), dinfo.Amount.ToString(), GenDate.SecondsToTicks(1));
            }
        }

        public override void Tick()
        {
            base.Tick();

            // check every second
            if (this.IsHashIntervalTick(GenDate.SecondsToTicks(1)))
            {
                // if dummy is not reserved by colonists
                if (!Find.Reservations.IsReserved(this, Faction))
                {
                    TryAssignTraining();
                }
            }
        }

        public override string GetInspectString()
        {
            var pawn = Find.Reservations.FirstReserverOf(this, Faction);
            var inspectString = new StringBuilder();
            if (pawn?.equipment.Primary != null && pawn.Position == InteractionCell)
            {
                var attackVerb = pawn.equipment.PrimaryEq.PrimaryVerb as Verb_MeleeAttack;

                if (attackVerb != null && attackVerb.CanHitTarget(this))
                {
                    inspectString.AppendFormat("{0} melee skill level:\t\t{1} ({2})\n", pawn.NameStringShort, pawn.skills.GetSkill(SkillDefOf.Melee).LevelDescriptor, pawn.skills.GetSkill(SkillDefOf.Melee).level);
                    inspectString.AppendLine("Melee hit chance:\t\t\t" + GenText.AsPercent(pawn.GetStatValue(StatDefOf.MeleeHitChance)));
                    inspectString.AppendLine("DPS with current accuracy:\t\t" + (pawn.GetStatValue(StatDefOf.MeleeHitChance) * pawn.GetStatValue(StatDefOf.MeleeWeapon_DamageAmount) / pawn.GetStatValue(StatDefOf.MeleeWeapon_Cooldown)).ToString("F1"));
                    if (pawn.skills.GetSkill(SkillDefOf.Shooting).level < 10)
                    {
                        inspectString.AppendLine("ExpPS with current weapon:\t\t" + (pawn.skills.GetSkill(SkillDefOf.Melee).LearningFactor * 10f / pawn.GetStatValue(StatDefOf.MeleeWeapon_Cooldown)).ToString("F1"));
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

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.LookList(ref allowedPawns, "allowedPawns", LookMode.MapReference);
        }
    }
}