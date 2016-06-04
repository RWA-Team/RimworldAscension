using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_PutInSlot : JobDriver
    {
        //Constants
        public const TargetIndex HaulableInd = TargetIndex.A;
        public const TargetIndex SlotterInd = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var slotter = CurJob.GetTarget(SlotterInd).Thing as ThingWithComps;
            var compSlots = slotter.GetComp<CompSlots>();

            // no free slots
            this.FailOn(() => compSlots.slots.Count >= compSlots.Properties.maxSlots);

            // reserve resources
            yield return Toils_Reserve.ReserveQueue(HaulableInd);

            // extract next target thing from targetQueue
            var toilExtractNextTarget = Toils_JobTransforms.ExtractNextTargetFromQueue(HaulableInd);
            yield return toilExtractNextTarget;

            var toilGoToThing = Toils_Goto.GotoThing(HaulableInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedOrNull(HaulableInd);
            yield return toilGoToThing;

            var pickUpThingIntoSlot = new Toil
            {
                initAction = () =>
                {
                    if (!compSlots.slots.TryAdd(CurJob.targetA.Thing))
                        EndJobWith(JobCondition.Incompletable);
                }
            };
            yield return pickUpThingIntoSlot;

            yield return Toils_Jump.JumpIfHaveTargetInQueue(HaulableInd, toilExtractNextTarget);
        }
    }
}