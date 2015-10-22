using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld.SquadAI;

namespace RA
{
    public class TransitionAction_SetDefendLocalGroup : TransitionAction
    {
        public TransitionAction_SetDefendLocalGroup() { }

        public override void DoAction(Transition trans)
        {
            State_DefendPoint position = (State_DefendPoint)trans.target;
            position.defendPoint = position.brain.ownedPawns.RandomElement<Pawn>().Position;
        }
    }
}
