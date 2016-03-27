using RimWorld;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;

namespace RA
{
    internal class State_DefendPoint : State
    {
        public IntVec3 defendPoint;
        public float defendRadius = 28f;

        public override IntVec3 FlagLoc => defendPoint;

        public State_DefendPoint()
        {
        }

        public State_DefendPoint(IntVec3 defendPoint)
        {
            this.defendPoint = defendPoint;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue(ref defendPoint, "defendPoint", default(IntVec3));
        }

        public override void UpdateAllDuties()
        {
            foreach (var pawn in brain.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, defendPoint, -1f)
                {
                    focusSecond = defendPoint,
                    radius = defendRadius
                };
            }
        }
    }
}