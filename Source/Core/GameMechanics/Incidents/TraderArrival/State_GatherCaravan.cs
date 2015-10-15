using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using RimWorld.SquadAI;

namespace RimworldAscension
{
    // Defend the position near the trading post
    public class State_GatherCaravan : State
    {
        public Building_TradingPost tradingPost;
        public IntVec3 gatherDest, animalDest;
        public Pawn merchant, animal;

        public State_GatherCaravan()
        {
        }

        public State_GatherCaravan(Building_TradingPost tradingPost)
        {
            this.tradingPost = tradingPost;
            this.gatherDest = tradingPost.InteractionCell;
            this.animalDest = new IntVec3(gatherDest.x, gatherDest.y, gatherDest.z + 1);
        }

        public override void Init()
        {
            merchant = brain.ownedPawns[0];
            animal = brain.ownedPawns[1];
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<Building_TradingPost>(ref tradingPost, "tradingPost");
        }

        public override void Cleanup()
        {
            tradingPost.merchant = null;
            tradingPost.occupied = false;

            // Gathering tradeables from trading post
            animal.TryGetComp<CompCaravan>().cargo = new List<Thing>(TradeSession.tradeCompany.things);
            TradeSession.tradeCompany.things.Clear();
        }

        public override void UpdateAllDuties()
        {
            merchant.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("GoTo"), gatherDest);
            animal.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("GoTo"), animalDest);

            // Duties for guards
            for (int i = 2; i < this.brain.ownedPawns.Count; i++)
            {
                brain.ownedPawns[i].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("GoTo"), CellFinder.RandomClosewalkCellNear(gatherDest, 3));
            }
        }
        public override void StateTick()
        {
            // if animal reached it's destination
            if (animal.Position != animalDest)
            {
                // skip trigger
                return;
            }
            else
            {
                // cycle until all pawns in range
                foreach (Pawn pawn in brain.ownedPawns)
                {
                    if (!pawn.Position.InHorDistOf(gatherDest, 3f))
                    {
                        // skip trigger
                        return;
                    }
                }
            }

            this.brain.ReceiveMemo("CaravanGathered");
        }
    }
}
