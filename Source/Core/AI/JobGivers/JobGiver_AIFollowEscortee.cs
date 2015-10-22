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
    public class JobGiver_AIFollowEscortee : JobGiver_AIFollowPawn
    {
        protected override Pawn GetFollowee(Pawn pawn)
        {
            return (Pawn)pawn.mindState.duty.focus.Thing;
        }

        protected override float GetRadius(Pawn pawn)
        {
            return pawn.mindState.duty.radius;
        }
    }
}
