using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_HaulToTrade : JobDriver
    {
        public const TargetIndex CarryThingIndex = TargetIndex.A;
        public const TargetIndex DestIndex = TargetIndex.B;

        public override string GetReport()
        {
            var hauledThing = pawn.carrier.CarriedThing ?? TargetThingA;

            return "ReportHaulingTo".Translate(hauledThing.LabelCap, CurJob.targetB.Thing.LabelBaseShort);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyed(CarryThingIndex);
            this.FailOnForbidden(CarryThingIndex);
            this.FailOnDestroyed(DestIndex);
            this.FailOnForbidden(DestIndex);

            yield return Toils_Reserve.Reserve(CarryThingIndex);
            //yield return Toils_Reserve.ReserveQueue(CarryThingIndex);

            AdjustRequestLists();

            // NOTE: use pickup queues later?
            var goToThing = Toils_Goto.GotoThing(CarryThingIndex, PathEndMode.ClosestTouch);
            yield return goToThing;

            yield return Toils_Haul.StartCarryThing(CarryThingIndex);

            //yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(reachHaulable, CarryThingIndex);

            yield return Toils_Haul.CarryHauledThingToContainer();

            yield return DepositHauledThingInContainer(DestIndex);
        }

        public Toil DepositHauledThingInContainer(TargetIndex containerInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                if (actor.carrier.CarriedThing == null)
                {
                    Log.Error(actor + " tried to place hauled thing in container but is not hauling anything.");
                    return;
                }
                var thingContainerOwner = curJob.GetTarget(containerInd).Thing as IThingContainerOwner;
                if (thingContainerOwner != null)
                {
                    var num = actor.carrier.CarriedThing.stackCount;
                    actor.carrier.container.TransferToContainer(actor.carrier.CarriedThing, thingContainerOwner.GetContainer(), num);
                    AdjustOfferedLists();
                }
                else if (curJob.GetTarget(containerInd).Thing.def.Minifiable)
                {
                    actor.carrier.container.Clear();
                }
                else
                {
                    Log.Error("Could not deposit hauled thing in container: " + curJob.GetTarget(containerInd).Thing);
                }
            };
            return toil;
        }

        public void AdjustRequestLists()
        {
            if (TargetThingA.def.stackLimit > 1)
            {
                var counter = WorkGiver_HaulToTrade.requestedResourceCounters.Find(item => item.thingDef == TargetThingA.def);
                counter.count -= CurJob.maxNumToCarry;
                if (counter.count == 0)
                    WorkGiver_HaulToTrade.requestedResourceCounters.Remove(counter);
            }
            else
                WorkGiver_HaulToTrade.requestedItems.Remove(TargetThingA);
        }

        public void AdjustOfferedLists()
        {
            var tradingPost = TargetThingB as Building_TradingPost;
            if (TargetThingA.def.stackLimit > 1)
            {
                var counter = tradingPost.offeredResourceCounters.Find(item => item.thingDef == TargetThingA.def);
                counter.count -= CurJob.maxNumToCarry;
                if (counter.count == 0)
                    tradingPost.offeredResourceCounters.Remove(counter);
            }
            else
                tradingPost.offeredItems.Remove(TargetThingA);
        }
    }
}

