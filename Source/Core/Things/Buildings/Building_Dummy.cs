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
    public class Building_Dummy : Building
    {
        public List<Pawn> allowedPawns = new List<Pawn>();
        public static List<Pawn> allowedPawns_Transfer;

        public virtual void FindTrainee()
        {
            foreach (Pawn pawn in allowedPawns)
            {
                // find pawn with equipped melee weapon
                if (pawn.equipment.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_MeleeAttack)
                    // which is also idle, has no life needs and can reserve and reach
                    if (pawn.mindState.IsIdle && !PawnHasNeeds(pawn) && ReservationUtility.CanReserveAndReach(pawn, this, PathEndMode.InteractionCell, Danger.Some))
                    {
                        pawn.drafter.TakeOrderedJob(new Job(DefDatabase<JobDef>.GetNamed("CombatTraining"), this, this.InteractionCell));
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
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            Command_Action gizmo_copySettings = new Command_Action
            {
                defaultDesc = "Copy dummy's settings",
                defaultLabel = "Copy Settings",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings", true),
                activateSound = SoundDef.Named("Click"),
                action = new Action(CopySettings),
            };
            yield return gizmo_copySettings;

            Command_Action gizmo_pasteSettings = new Command_Action
            {
                defaultDesc = "Paste dummy's settings",
                defaultLabel = "Paste Settings",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings", true),
                activateSound = SoundDef.Named("Click"),
                action = new Action(PasteSettings),
            };
            yield return gizmo_pasteSettings;
        }

        public virtual void CopySettings()
        {
            Building_Dummy.allowedPawns_Transfer = new List<Pawn>(allowedPawns);
        }

        public virtual void PasteSettings()
        {
            if (Building_Dummy.allowedPawns_Transfer != null)
                allowedPawns = new List<Pawn>(Building_Dummy.allowedPawns_Transfer);
        }
                
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            // if zoom is close enough and dummy is selected
            if (Find.CameraMap.CurrentZoom == CameraZoomRange.Closest && Find.Selector.IsSelected(this))
                // throws text mote with applied damage each time damage taken
                MoteThrower.ThrowText(new Vector3(this.Position.x + 0.5f, this.Position.y, this.Position.z + 0.8f), dinfo.Amount.ToString(), 60);
        }

        public override void Tick()
        {
            base.Tick();

            // check every 30 ticks
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                // if dummy is not reserved by colonists
                if (!Find.Reservations.IsReserved(this, Faction))
                {
                    FindTrainee();
                }
            }
        }

        public override string GetInspectString()
        {
            Pawn pawn = Find.Reservations.FirstReserverOf(this, Faction);
            StringBuilder inspectString = new StringBuilder();
            if (pawn != null && pawn.equipment.Primary != null && pawn.Position == InteractionCell)
            {
                Verb_MeleeAttack attackVerb = pawn.equipment.PrimaryEq.PrimaryVerb as Verb_MeleeAttack;

                if (attackVerb != null && attackVerb.CanHitTarget(this))
                {
                    inspectString.AppendLine(pawn.NameStringShort + " melee skill level:\t" + pawn.skills.GetSkill(SkillDefOf.Melee).LevelDescriptor);
                    inspectString.AppendLine("Melee hit chance:\t" + GenText.AsPercent(pawn.GetStatValue(StatDefOf.MeleeHitChance)));
                    inspectString.AppendLine("Current DPS:\t" + pawn.GetStatValue(StatDefOf.MeleeHitChance) * pawn.GetStatValue(StatDefOf.MeleeWeapon_DamageAmount) / pawn.GetStatValue(StatDefOf.MeleeWeapon_Cooldown));
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

            Scribe_Collections.LookList<Pawn>(ref allowedPawns, "allowedPawns", LookMode.MapReference, new object[0]);
        }
    }
}