using RimWorld.SquadAI;
using Verse;
using Verse.AI;

namespace RA
{
    public class State_Travel : State
    {
        public Building_TradingPost tradingPost;
        public IntVec3 merchantDest, animalDest;
        public Pawn merchant, animal;


        public State_Travel()
        {
        }

        public State_Travel(Building_TradingPost tradingPost)
        {
            this.tradingPost = tradingPost;
            // one cell above the interaction cell of trading post
            merchantDest = new IntVec3(tradingPost.InteractionCell.x, tradingPost.InteractionCell.y, tradingPost.InteractionCell.z + 1);
            // one cell above and left of the interaction cell of trading post
            animalDest = tradingPost.InteractionCell;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference(ref tradingPost, "tradingPost");
        }

        public override void UpdateAllDuties()
        {

            for (var i = 0; i < brain.ownedPawns.Count; i++)
            {
                // merchant
                if (brain.ownedPawns[i].kindDef.label == "merchant")
                {
                    merchant = brain.ownedPawns[i];
                    merchant.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("GoTo"), merchantDest);
                }

                // animal
                else if (brain.ownedPawns[i].def.defName == "CaravanMuffalo")
                {
                    animal = brain.ownedPawns[i];
                    animal.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("GoTo"), animalDest);
                }

                // guards
                else
                {
                    brain.ownedPawns[i].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("EscortTradeCart"), merchant, 3);
                }
            }
        }

        public override void StateTick()
        {
            if (Find.TickManager.TicksGame % 205 == 0)
            {
                if (merchant.Position == merchantDest && animal.Position == animalDest)
                {
                    brain.ReceiveMemo("TravelArrived");
                }
            }
        }
    }
}