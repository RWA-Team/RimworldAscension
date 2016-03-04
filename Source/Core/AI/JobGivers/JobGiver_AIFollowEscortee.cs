using Verse;

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
