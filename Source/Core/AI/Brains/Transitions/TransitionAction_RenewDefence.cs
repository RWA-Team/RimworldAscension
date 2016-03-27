
using RimWorld;
using RimWorld.SquadAI;
using Verse;

namespace RA
{
    public class TransitionAction_RenewDefence : TransitionAction
    {
        public override void DoAction(Transition trans)
        {
            var position = (State_DefendPoint) trans.target;
            position.defendPoint = position.brain.ownedPawns.RandomElement().Position;

            var timer = trans.triggers.Find(trigger => trigger.GetType() == typeof (Trigger_TicksPassed)) as Trigger_TicksPassed;
            timer.duration = GenDate.TicksPerHour;
        }
    }
}
