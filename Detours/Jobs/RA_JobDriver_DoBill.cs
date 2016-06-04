using UnityEngine;
using Verse;
using Verse.AI;

namespace RimworldAscension.Core.Detours.Jobs
{
    public static class RA_JobDriver_DoBill
    {
        // fixes bug for decreased carry capacity
        public static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                const float MaxDist = 8;
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var targetQueue = curJob.GetTargetQueue(ind);

                if (targetQueue.NullOrEmpty())
                    return;

                if (actor.carrier.CarriedThing == null)
                {
                    Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.");
                    return;
                }

                //Find an item in the queue matching what you're carrying
                for (var i = 0; i < targetQueue.Count; i++)
                {
                    //Can't use item - skip
                    if (!GenAI.CanUseItemForWork(actor, targetQueue[i].Thing))
                        continue;

                    //Cannot stack with thing in hands - skip
                    if (!targetQueue[i].Thing.CanStackWith(actor.carrier.CarriedThing))
                        continue;

                    //Too far away - skip
                    if ((actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared > MaxDist * MaxDist)
                        continue;

                    //Determine num in hands
                    var numInHands = actor.carrier.CarriedThing?.stackCount ?? 0;

                    //Determine num to take
                    var numToTake = Mathf.Min(curJob.numToBringList[i], actor.carrier.AvailableStackSpace(targetQueue[i].Thing.def));

                    //Won't take any - skip
                    if (numToTake == 0)
                        continue;

                    //Remove the amount to take from the num to bring list
                    curJob.numToBringList[i] -= numToTake;

                    //Set pawn to go get it
                    curJob.maxNumToCarry = numInHands + numToTake;
                    curJob.SetTarget(ind, targetQueue[i].Thing);

                    //Remove from queue if I'm going to take all
                    if (curJob.numToBringList[i] == 0)
                    {
                        curJob.numToBringList.RemoveAt(i);
                        targetQueue.RemoveAt(i);
                    }

                    actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
                    return;
                }

            };

            return toil;
        }
    }
}
