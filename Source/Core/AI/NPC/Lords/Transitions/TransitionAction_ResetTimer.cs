using Verse.AI.Group;

namespace RA
{
    public class TransitionAction_ResetTimer : TransitionAction
    {
        public int baseTicks;

        public TransitionAction_ResetTimer(int baseTicks)
        {
            this.baseTicks = baseTicks;
        }

        public override void DoAction(Transition trans)
        {
            trans.triggers.RemoveAll(trigger => trigger.GetType() == typeof (Trigger_TicksPassed));
            trans.triggers.Add(new Trigger_TicksPassed(baseTicks));
        }
    }
}
