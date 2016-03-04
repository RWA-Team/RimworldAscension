
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public abstract class JobGiver_AIFollowPawn : ThinkNode_JobGiver
	{
		protected virtual int FollowJobExpireInterval => 400;

        protected abstract Pawn GetFollowee(Pawn pawn);

		protected abstract float GetRadius(Pawn pawn);

		protected override Job TryGiveTerminalJob(Pawn pawn)
		{
			var followee = GetFollowee(pawn);
			if (!GenAI.CanInteractPawn(pawn, followee))
			{
				return null;
			}
			var radius = GetRadius(pawn);
			if ((followee.pather.Moving && followee.pather.Destination.Cell.DistanceToSquared(pawn.Position) > radius * radius) || followee.Position.GetRoom() != pawn.GetRoom() || followee.Position.DistanceToSquared(pawn.Position) > radius * radius)
			{
				var root = followee.Position;

				var vec = CellFinder.RandomClosewalkCellNear(root, Mathf.RoundToInt(radius * 0.7f));
				return new Job(JobDefOf.Goto, vec)
				{
                    locomotionUrgency = LocomotionUrgency.Walk,
					expiryInterval = FollowJobExpireInterval,
					checkOverrideOnExpire = true
				};
			}
			return null;
		}
	}
}
