using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

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
                if (pawn.equipment.Primary == null)
                {
                    return true;
                }
                return false;
            });

            yield return Toils_Reserve.Reserve(DummyInd);
            yield return Toils_Goto.GotoThing(DummyInd, PathEndMode.InteractionCell);
            yield return PractiseCombat(DummyInd);
        }

        public Toil PractiseCombat(TargetIndex targetInd)
        {
            var toil = new Toil();

            var lastMeleeExperienceValue = pawn.skills.GetSkill(SkillDefOf.Melee).XpTotalEarned + pawn.skills.GetSkill(SkillDefOf.Melee).xpSinceLastLevel;
            var lastShootingExperienceValue = pawn.skills.GetSkill(SkillDefOf.Shooting).XpTotalEarned + pawn.skills.GetSkill(SkillDefOf.Shooting).xpSinceLastLevel;

            toil.tickAction = () =>
            {
                // try execute attack on dummy
                pawn.equipment.TryStartAttack(CurJob.GetTarget(targetInd));

                // if zoom is close enough and dummy is selected
                if (Find.CameraMap.CurrentZoom == CameraZoomRange.Closest && (Find.Selector.IsSelected(CurJob.GetTarget(targetInd).Thing) || Find.Selector.IsSelected(pawn)))
                {
                    var currentMeleeExperienceValue = pawn.skills.GetSkill(SkillDefOf.Melee).XpTotalEarned + pawn.skills.GetSkill(SkillDefOf.Melee).xpSinceLastLevel;
                    var currentShootingExperienceValue = pawn.skills.GetSkill(SkillDefOf.Shooting).XpTotalEarned + pawn.skills.GetSkill(SkillDefOf.Shooting).xpSinceLastLevel;

                    // throws text mote of gained melee experience
                    if (currentMeleeExperienceValue - lastMeleeExperienceValue >= 1f)
                    {
                        var expGained = currentMeleeExperienceValue - lastMeleeExperienceValue;
                        MoteThrower.ThrowText(new Vector3(pawn.Position.x + 0.5f, pawn.Position.y, pawn.Position.z + 1f), expGained.ToString("F0") + " XP", Color.green, GenDate.TicksPerRealSecond);
                        lastMeleeExperienceValue = currentMeleeExperienceValue;
                    }

                    // throws text mote of gained shooting experience
                    if (currentShootingExperienceValue - lastShootingExperienceValue >= 1f)
                    {
                        var expGained = currentShootingExperienceValue - lastShootingExperienceValue;
                        MoteThrower.ThrowText(new Vector3(pawn.Position.x + 0.5f, pawn.Position.y, pawn.Position.z + 1f), expGained.ToString("F0") + " XP", Color.green, GenDate.TicksPerRealSecond);
                        lastShootingExperienceValue = currentShootingExperienceValue;
                    }
                }
            };

            toil.AddEndCondition(() =>
            {
                // fail if pawn has life needs or can't hit target
                var dummy = CurJob.GetTarget(targetInd).Thing as Dummy;
                if (dummy != null && (dummy.PawnHasNeeds(pawn) || !pawn.equipment.PrimaryEq.PrimaryVerb.CanHitTarget(CurJob.GetTarget(targetInd))))
                {
                    return JobCondition.InterruptForced;
                }
                return JobCondition.Ongoing;
            });
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }
    }
}
