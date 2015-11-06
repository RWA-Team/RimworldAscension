using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using Verse.Sound;


namespace RA
{
    class JobDriver_CombatTraining : JobDriver
    {
        public const TargetIndex DummyInd = TargetIndex.A;
        public const TargetIndex InteractionCellInd = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyed(DummyInd);
            // fail if interaction cell is burning
            this.FailOnBurningImmobile(InteractionCellInd);
            // fail if pawn has no primary weapon
            this.FailOn(() =>
            {
                if (this.pawn.equipment.Primary == null)
                {
                    return true;
                }
                else
                    return false;
            });

            yield return Toils_Reserve.Reserve(DummyInd);
            yield return Toils_Goto.GotoThing(DummyInd, PathEndMode.InteractionCell);
            yield return PractiseCombat(DummyInd);
        }

        public Toil PractiseCombat(TargetIndex targetInd)
        {
            Toil toil = new Toil();

            float lastMeleeExperienceValue = this.pawn.skills.GetSkill(SkillDefOf.Melee).XpTotalEarned + this.pawn.skills.GetSkill(SkillDefOf.Melee).xpSinceLastLevel;
            float lastShootingExperienceValue = this.pawn.skills.GetSkill(SkillDefOf.Shooting).XpTotalEarned + this.pawn.skills.GetSkill(SkillDefOf.Shooting).xpSinceLastLevel;

            toil.tickAction = () =>
            {
                // try execute attack on dummy
                this.pawn.equipment.TryStartAttack(CurJob.GetTarget(targetInd));

                // if zoom is close enough and dummy is selected
                if (Find.CameraMap.CurrentZoom == CameraZoomRange.Closest && (Find.Selector.IsSelected(CurJob.GetTarget(targetInd).Thing) || Find.Selector.IsSelected(this.pawn)))
                {
                    float currentMeleeExperienceValue = this.pawn.skills.GetSkill(SkillDefOf.Melee).XpTotalEarned + this.pawn.skills.GetSkill(SkillDefOf.Melee).xpSinceLastLevel;
                    float currentShootingExperienceValue = this.pawn.skills.GetSkill(SkillDefOf.Shooting).XpTotalEarned + this.pawn.skills.GetSkill(SkillDefOf.Shooting).xpSinceLastLevel;

                    // throws text mote of gained melee experience
                    if ((currentMeleeExperienceValue - lastMeleeExperienceValue) >= 1f)
                    {
                        float expGained = currentMeleeExperienceValue - lastMeleeExperienceValue;
                        MoteThrower.ThrowText(new Vector3(this.pawn.Position.x + 0.5f, this.pawn.Position.y, this.pawn.Position.z + 1f), expGained.ToString("F0") + " XP", Color.green, GenDate.TicksPerRealSecond);
                        lastMeleeExperienceValue = currentMeleeExperienceValue;
                    }

                    // throws text mote of gained shooting experience
                    if ((currentShootingExperienceValue - lastShootingExperienceValue) >= 1f)
                    {
                        float expGained = currentShootingExperienceValue - lastShootingExperienceValue;
                        MoteThrower.ThrowText(new Vector3(this.pawn.Position.x + 0.5f, this.pawn.Position.y, this.pawn.Position.z + 1f), expGained.ToString("F0") + " XP", Color.green, GenDate.TicksPerRealSecond);
                        lastShootingExperienceValue = currentShootingExperienceValue;
                    }
                }
            };

            toil.AddEndCondition(() =>
            {
                // fail if pawn has life needs or can't hit target
                if ((CurJob.GetTarget(targetInd).Thing as Dummy).PawnHasNeeds(this.pawn) || !this.pawn.equipment.PrimaryEq.PrimaryVerb.CanHitTarget(CurJob.GetTarget(targetInd)))
                {
                    return JobCondition.InterruptForced;
                }
                else
                    return JobCondition.Ongoing;
            });
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }
    }
}
