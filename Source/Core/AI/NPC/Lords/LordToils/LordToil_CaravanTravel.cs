using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RA
{
	public class LordToil_CaravanTravel : LordToil
    {
        public const float AllArrivedCheckRadius = 20f;

        public LordToilData_Travel Data => (LordToilData_Travel)data;
        public override IntVec3 FlagLoc => Data.dest;
	    public override bool AllowSatisfyLongNeeds => false;


	    public LordToil_CaravanTravel(IntVec3 dest)
		{
            // standart initialization of data instance
			data = new LordToilData_Travel();
			Data.dest = dest;
		}

	    public override void UpdateAllDuties()
        {
            // find first pawn of trader role
            var trader = TraderCaravanUtility.FindTrader(lord);
	        if (trader != null)
	        {
	            trader.mindState.duty = new PawnDuty(DutyDefOf.Travel, Data.dest)
                {
                    locomotion = LocomotionUrgency.Walk,
                    maxDanger = trader.NormalMaxDanger()
	            };
	            foreach (var pawn in lord.ownedPawns)
	            {
	                if (pawn == trader)
	                    continue;

	                switch (pawn.GetCaravanRole())
	                {
	                    case TraderCaravanRole.Guard:
	                        pawn.mindState.duty = new PawnDuty(DutyDefOf.Escort, trader, 5f)
	                        {
	                            locomotion = LocomotionUrgency.Walk
	                        };
	                        break;
	                    default:
	                        pawn.mindState.duty = new PawnDuty(DutyDefOf.Follow, trader, 8f)
                            {
                                locomotion = LocomotionUrgency.Walk
                            };
                            break;
	                }
	            }
	        }
	    }

	    public override void LordToilTick()
        {
            if (Find.TickManager.TicksGame % 205 == 0)
            {
                var arrived = lord.ownedPawns.All(pawn => pawn.Position.InHorDistOf(FlagLoc, AllArrivedCheckRadius) && pawn.CanReach(FlagLoc, PathEndMode.ClosestTouch, Danger.Deadly));
                if (arrived)
                {
                    lord.ReceiveMemo("TravelArrived");
                }
            }
        }
	}
}
