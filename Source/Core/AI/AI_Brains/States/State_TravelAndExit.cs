using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;
using RimWorld.SquadAI;

namespace RA
{

    /// <summary>
    /// State: Travel to a position and then leave the map
    /// </summary>
    class State_TravelAndExit : State
    {
        public IntVec3 destCell;
        public bool destAssigned;

        public override IntVec3 FlagLoc
        {
            get
            {
                return destCell;
            }
        }


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
            IntVec3 intVec3 = new IntVec3();
            Scribe_Values.LookValue<IntVec3>(ref destCell, "dest", intVec3, false);
            Scribe_Values.LookValue<bool>(ref destAssigned, "destAssigned", false, false);
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
                bool flag = true;
                int num = 0;
                while (num < brain.ownedPawns.Count)
                {
                    Pawn item = brain.ownedPawns[num];
                    if (!item.Position.InHorDistOf(destCell, 10f) || !item.CanReach(destCell, PathEndMode.OnCell, Danger.Deadly))
                    {
                        flag = false;
                        break;
                    }
                    else
                    {
                        num++;
                    }
                }
                if (flag)
                {
                    GiveDutyExitMap();
                }
            }
        }

        public override void UpdateAllDuties()
        {
            foreach (Pawn ownedPawn in this.brain.ownedPawns)
            {
                ownedPawn.mindState.duty = new PawnDuty(DutyDefOf.Travel, destCell);
            }
        }

        
        public void GiveDutyExitMap()
        {
            foreach (Pawn ownedPawn in brain.ownedPawns)
            {
                ownedPawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapNearest);
            }
        }


    }
}
