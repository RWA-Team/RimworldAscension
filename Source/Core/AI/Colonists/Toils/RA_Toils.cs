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
    }
}
