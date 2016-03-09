using System.Collections.Generic;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;

namespace RA
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
            gatherDest = tradingPost.InteractionCell;
            animalDest = new IntVec3(gatherDest.x, gatherDest.y, gatherDest.z + 1);
        }

        public override void Init()
        {
            merchant = brain.ownedPawns[0];
            animal = brain.ownedPawns[1];
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference(ref tradingPost, "tradingPost");
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
            for (var i = 2; i < brain.ownedPawns.Count; i++)
            {
                brain.ownedPawns[i].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("GoTo"), CellFinder.RandomClosewalkCellNear(gatherDest, 3));
            }
        }
        public override void StateTick()
        {
            // if animal reached it's destination
            if (animal.Position == animalDest)
            {
                foreach (var pawn in brain.ownedPawns)
                {
                    if (!pawn.Position.InHorDistOf(gatherDest, 3f))
                    {
                        // skip trigger
                        return;
                    }
                }

                brain.ReceiveMemo("CaravanGathered");
            }
            // cycle until all pawns in range
        }
    }
}
