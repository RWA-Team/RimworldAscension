
using RimWorld.SquadAI;
using Verse;

namespace RA
{
    public class TransitionAction_SetDefendLocalGroup : TransitionAction
    {
        public override void DoAction(Transition trans)
        {
            var position = (State_DefendPoint)trans.target;
            position.defendPoint = position.brain.ownedPawns.RandomElement().Position;
        }
    }
}
