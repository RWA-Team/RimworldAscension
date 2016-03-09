using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;

namespace RA
{
    public class SnatchTarget : State
    {
        private List<Thing> snatchTargets = new List<Thing>();

        public SnatchTarget()
        {
            foreach (Pawn snatcher in brain.ownedPawns)
            {
                var victim = KidnapAIUtility.ClosestKidnapVictim(snatcher, 9999f);
                if (victim != null)
                    snatchTargets.Add(victim);

                Find.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
            }

            //animalDest = tradingPost.InteractionCell;
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