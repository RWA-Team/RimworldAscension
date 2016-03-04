using RimWorld;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;
//using RimWorld.Planet;

namespace RA
{

    /// <summary>
    /// State: Travel to a position and then leave the map
    /// </summary>
    class State_TravelAndExit : State
    {
        public IntVec3 destCell;
        public bool destAssigned;

        public override IntVec3 FlagLoc => destCell;


        /// <summary>
        /// Init
        /// </summary>
        public State_TravelAndExit()
        {
        }

        public State_TravelAndExit(IntVec3 dest)
        {
            destCell = dest;
            destAssigned = true;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            var intVec3 = new IntVec3();
            Scribe_Values.LookValue(ref destCell, "dest", intVec3);
            Scribe_Values.LookValue(ref destAssigned, "destAssigned", false);
        }


        public override void Init()
        {
            base.Init();
            if (!destAssigned)
            {
                if (!RCellFinder.TryFindTravelDestFrom(brain.ownedPawns[0].Position, out destCell))
                {
                    Log.Error(string.Concat("Travelers for ", brain.faction, " could not late-find travel destination."));
                    destCell = brain.ownedPawns[0].Position;
                }
                destAssigned = true;
            }
        }

        public override void Cleanup()
        {
            TradeSession.tradeCompany = null;
        }


        public override void StateTick()
        {
            if (Find.TickManager.TicksGame % 205 == 0)
            {
                var flag = true;
                var num = 0;
                while (num < brain.ownedPawns.Count)
                {
                    var pawn = brain.ownedPawns[num];
                    if (!pawn.Position.InHorDistOf(destCell, 10f) || !pawn.CanReach(destCell, PathEndMode.OnCell, pawn.NormalMaxDanger()))
                    {
                        flag = false;
                        break;
                    }
                    num++;
                }
                if (flag)
                {
                    GiveDutyExitMap();
                }
            }
        }

        public override void UpdateAllDuties()
        {
            foreach (var ownedPawn in brain.ownedPawns)
            {
                ownedPawn.mindState.duty = new PawnDuty(DutyDefOf.Travel, destCell);
            }
        }

        
        public void GiveDutyExitMap()
        {
            foreach (var ownedPawn in brain.ownedPawns)
            {
                ownedPawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapNearest);
            }
        }


    }
}
