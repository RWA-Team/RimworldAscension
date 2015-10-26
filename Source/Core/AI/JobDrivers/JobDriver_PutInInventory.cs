using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;


namespace RA
{/*
    public class JobDriver_PutInInventory : JobDriver
    {
        //Constants
        public const TargetIndex BackpackInd = TargetIndex.A;
        public const TargetIndex HaulableInd = TargetIndex.B;

        public JobDriver_PutInInventory() : base() { }

        public override string GetReport()
        {
            Thing hauledThing = TargetThingB;

            string repString;
            if (hauledThing != null)
                repString = "ReportPutInInventory".Translate(hauledThing.LabelCap, CurJob.GetTarget(BackpackInd).Thing.LabelCap);
            else
                repString = "ReportPutSomethingInInventory".Translate(CurJob.GetTarget(BackpackInd).Thing.LabelCap);

            return repString;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Apparel_Backpack backpack = CurJob.GetTarget(BackpackInd).Thing as Apparel_Backpack;

            ///
            //Set fail conditions
            ///


            //Backpack is full.
            this.FailOn(() =>{ return (pawn.inventory.container.Count < backpack.Capacity) ? false : true; });

            ///
            //Define Toil
            ///

            Toil extractB = new Toil();
            extractB.initAction = () =>
            {
                if (!CurJob.targetQueueB.NullOrEmpty())
                {
                    CurJob.targetB = CurJob.targetQueueB.First();
                    CurJob.targetQueueB.RemoveAt(0);
                    this.FailOnDestroyed(HaulableInd);
                }
                else
                    this.EndJobWith(JobCondition.Succeeded);
            };

            Toil toilGoToThing = Toils_Goto.GotoThing(HaulableInd, PathEndMode.ClosestTouch)
                                    .FailOnDespawned(HaulableInd);

            ///
            //Toils Start
            ///

            //Reserve thing to be stored and storage cell 
            yield return Toils_Reserve.Reserve(BackpackInd);
            yield return Toils_Reserve.Reserve(HaulableInd);
            yield return Toils_Reserve.ReserveQueue(HaulableInd);


            yield return Toils_Jump.JumpIf(toilGoToThing, () => { return (CurJob.targetB.HasThing) ? true : false;  });

            //Collect TargetQueue
            {

                //Extract an haulable into TargetA
                yield return extractB;

                yield return toilGoToThing;

                //CollectIntoCarrier
                Toil toilPutInInventory = new Toil();
                toilPutInInventory.initAction = () => 
                {
                    if (pawn.inventory.container.Count < backpack.Capacity
                        && backpack.wearer.inventory.container.TotalStackCount < backpack.MaxStack)
                    {
                        if (CurJob.targetB.Thing.TryGetComp<CompForbiddable>() != null && CurJob.targetB.Thing.TryGetComp<CompForbiddable>().Forbidden == true)
                            CurJob.targetB.Thing.TryGetComp<CompForbiddable>().Forbidden = false;
                        if (pawn.inventory.container.TryAdd(CurJob.targetB.Thing, CurJob.maxNumToCarry))
                        {
                            CurJob.targetB.Thing.holder = pawn.inventory.GetContainer();
                            CurJob.targetB.Thing.holder.owner = pawn.inventory;
                            backpack.numOfSavedItems++;
                        }
                    }
                    else
                        this.EndJobWith(JobCondition.Incompletable);
                };
                yield return toilPutInInventory;
                yield return Toils_Jump.JumpIfHaveTargetInQueue(HaulableInd, extractB);
            }
        }

    }*/
}
