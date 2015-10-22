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
                if (this.GetActor().equipment.Primary == null)
                {
                    return true;
                }
                else
                    return false;
            });

            yield return Toils_Reserve.Reserve(DummyInd);
            yield return Toils_Goto.GotoThing(DummyInd, PathEndMode.InteractionCell);
            yield return PractiseCombat(CurJob.targetA);
        }

        public Toil PractiseCombat(TargetInfo target)
        {
            Toil toil = new Toil();

            toil.tickAction = () =>
            {
                toil.actor.equipment.TryStartAttack(target);
            };

            toil.AddEndCondition(() =>
            {
                if ((target.Thing as Building_Dummy).PawnHasNeeds(toil.actor))
                {
                    return JobCondition.InterruptOptional;
                }
                else
                    return JobCondition.Ongoing;
            });
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }
    }
}
