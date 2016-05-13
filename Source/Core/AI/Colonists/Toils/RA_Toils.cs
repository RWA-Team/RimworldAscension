using System;
using Verse;
using Verse.AI;

namespace RA
{
    public static class RA_Toils
    {
        public static Toil DepositHauledThingInContainer(TargetIndex containerInd, Action<Thing> depositAction = null)
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
                    depositAction(actor.carrier.CarriedThing);
                    var num = actor.carrier.CarriedThing.stackCount;
                    actor.carrier.container.TransferToContainer(actor.carrier.CarriedThing, thingContainerOwner.GetContainer(), num);
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

        // ignites burner to start consume fuel, if needed
        public static Toil WaitUntilBurnerReady()
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                var actor = toil.actor;
                var burner = toil.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing as WorkTableFueled;

                if (burner == null)
                {
                    return;
                }
                actor.pather.StopDead();
            };
            toil.tickAction = () =>
            {
                var burner = toil.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing as WorkTableFueled;

                if (burner.internalTemp > burner.compFueled.Properties.operatingTemp)
                    toil.actor.jobs.curDriver.ReadyForNextToil();
            };
            // fails if no more heat generation and temperature is no enough
            toil.FailOn(() =>
            {
                var burner = toil.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing as WorkTableFueled;

                return burner.currentFuelBurnDuration == 0 && !burner.UsableNow;
            });
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }
    }
}
