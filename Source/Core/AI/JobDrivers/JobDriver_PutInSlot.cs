using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;


namespace RA
{
    public class JobDriver_PutInSlot : JobDriver
    {
        //Constants
        public const TargetIndex HaulableInd = TargetIndex.A;
        public const TargetIndex SlotterInd = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ThingWithComps slotter = CurJob.GetTarget(SlotterInd).Thing as ThingWithComps;
            CompSlots compSlots = slotter.GetComp<CompSlots>();

            // no free slots
            this.FailOn(() => { return (compSlots.slots.Count >= compSlots.Properties.maxSlots) ? true : false; });

            // reserve resources
            yield return Toils_Reserve.ReserveQueue(HaulableInd);

            // extract next target thing from targetQueue
            Toil toilExtractNextTarget = Toils_JobTransforms.ExtractNextTargetFromQueue(HaulableInd);
            yield return toilExtractNextTarget;

            Toil toilGoToThing = Toils_Goto.GotoThing(HaulableInd, PathEndMode.ClosestTouch)
                .FailOnDespawned(HaulableInd);
            yield return toilGoToThing;
            
            Toil pickUpThingIntoSlot = new Toil();
            pickUpThingIntoSlot.initAction = () =>
            {
                if (!compSlots.slots.TryAdd(CurJob.targetA.Thing))
                    this.EndJobWith(JobCondition.Incompletable);
            };
            yield return pickUpThingIntoSlot;

            yield return Toils_Jump.JumpIfHaveTargetInQueue(HaulableInd, toilExtractNextTarget);
        }
    }
}
