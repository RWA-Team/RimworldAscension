using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            Thing hauledThing = null;
            if (pawn.carrier.CarriedThing != null)
                hauledThing = pawn.carrier.CarriedThing;
            else
                hauledThing = TargetThingA;

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
            Toil reachHaulable = Toils_Goto.GotoThing(CarryThingIndex, PathEndMode.ClosestTouch);
            yield return reachHaulable;

            yield return Toils_Haul.StartCarryThing(CarryThingIndex);

            //yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(reachHaulable, CarryThingIndex);

            yield return Toils_Haul.CarryHauledThingToContainer();

            yield return DepositHauledThingInContainer(DestIndex);
        }

        public Toil DepositHauledThingInContainer(TargetIndex containerInd)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                if (actor.carrier.CarriedThing == null)
                {
                    Log.Error(actor + " tried to place hauled thing in container but is not hauling anything.");
                    return;
                }
                IThingContainerOwner thingContainerOwner = curJob.GetTarget(containerInd).Thing as IThingContainerOwner;
                if (thingContainerOwner != null)
                {
                    int num = actor.carrier.CarriedThing.stackCount;
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
            Log.Message("target " + TargetThingA);
            Log.Message("num to carry " + CurJob.maxNumToCarry);
            if (TargetThingA.def.stackLimit > 1)
            {
                ThingCount counter = WorkGiver_HaulToTrade.requestedResourceCounters.Find(item => item.thingDef == TargetThingA.def);
                counter.count -= CurJob.maxNumToCarry;
                if (counter.count == 0)
                    WorkGiver_HaulToTrade.requestedResourceCounters.Remove(counter);
            }
            else
                WorkGiver_HaulToTrade.requestedItems.Remove(TargetThingA);
        }

        public void AdjustOfferedLists()
        {
            Building_TradingPost tradingPost = TargetThingB as Building_TradingPost;
            if (TargetThingA.def.stackLimit > 1)
            {
                ThingCount counter = tradingPost.offeredResourceCounters.Find(item => item.thingDef == TargetThingA.def);
                counter.count -= CurJob.maxNumToCarry;
                if (counter.count == 0)
                    tradingPost.offeredResourceCounters.Remove(counter);
            }
            else
                tradingPost.offeredItems.Remove(TargetThingA);
        }
    }
}

