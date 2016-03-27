using RimWorld.SquadAI;
using Verse;
using Verse.AI;

namespace RA
{
    public class State_SnatchGoodsOrPeople : State
    {
        public override void UpdateAllDuties()
        {
            foreach (var pawn in brain.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("SnatchGoodsOrPeople"));
            }
        }

        // when any target spotted go aggressive search mod

        public static IntVec3 RandomWanderPointNearColonyBuildings()
        {
            var building = Find.ListerBuildings.allBuildingsColonist.RandomElement();
            return CellFinder.RandomClosewalkCellNear(building.Position, (int)(building.RotatedSize.Magnitude+1));
        }
    }
}
