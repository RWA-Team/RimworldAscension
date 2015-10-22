using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.SquadAI;

namespace RA
{
	public abstract class JobGiver_AIFollowPawn : ThinkNode_JobGiver
	{
		protected virtual int FollowJobExpireInterval
		{
			get
			{
				return 400;
			}
		}

		protected abstract Pawn GetFollowee(Pawn pawn);

		protected abstract float GetRadius(Pawn pawn);

		protected override Job TryGiveTerminalJob(Pawn pawn)
		{
			Pawn followee = this.GetFollowee(pawn);
			if (!GenAI.CanInteractPawn(pawn, followee))
			{
				return null;
			}
			float radius = this.GetRadius(pawn);
			if ((followee.pather.Moving && followee.pather.Destination.Cell.DistanceToSquared(pawn.Position) > radius * radius) || followee.Position.GetRoom() != pawn.GetRoom() || followee.Position.DistanceToSquared(pawn.Position) > radius * radius)
			{
				IntVec3 root = followee.Position;

				IntVec3 vec = CellFinder.RandomClosewalkCellNear(root, Mathf.RoundToInt(radius * 0.7f));
				return new Job(JobDefOf.Goto, vec)
				{
                    locomotionUrgency = LocomotionUrgency.Walk,
					expiryInterval = this.FollowJobExpireInterval,
					checkOverrideOnExpire = true
				};
			}
			return null;
		}
	}
}
