using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RA
{
    // all Triggers are single use and need to initialized for each case of application
    // transition position in list matters
    public class RA_LordJob_TradeWithColony : LordJob
    {
        public Faction faction;
        // random cell near colony
        public IntVec3 chillSpot;

        // required for exposing
        public RA_LordJob_TradeWithColony() { }

        public RA_LordJob_TradeWithColony(Faction faction, IntVec3 chillSpot)
        {
            this.faction = faction;
            this.chillSpot = chillSpot;
        }

        public override StateGraph CreateGraph()
        {
            var mainGraph = new StateGraph();

            var arriveAtChillSpot = new LordToil_CaravanTravel(chillSpot);
            mainGraph.lordToils.Add(arriveAtChillSpot);

            var caravanStation = new LordToil_CaravanStation();
            mainGraph.lordToils.Add(caravanStation);
            var transitionToChill = new Transition(arriveAtChillSpot, caravanStation);
            transitionToChill.triggers.Add(new Trigger_Memo("TravelArrived"));
            mainGraph.transitions.Add(transitionToChill);

            var unloadGoods = new LordToil_UnloadGoods();
            mainGraph.lordToils.Add(unloadGoods);
            var transitionToUnloadGoods = new Transition(caravanStation, unloadGoods);
            transitionToUnloadGoods.triggers.Add(new Trigger_CanTrade());
            mainGraph.transitions.Add(transitionToUnloadGoods);

            var trade = new LordToil_Trade();
            mainGraph.lordToils.Add(trade);
            var transitionToTrade = new Transition(unloadGoods, trade);
            transitionToTrade.triggers.Add(new Trigger_Memo("TravelArrived"));
            // transit toil data to the next toil
            transitionToTrade.preActions.Add(new TransitionAction_Custom(() => trade.data = unloadGoods.Data));
            mainGraph.transitions.Add(transitionToTrade);

            var loadGoods = new LordToil_LoadGoods();
            mainGraph.lordToils.Add(loadGoods);
            var transitionToLoadGoods = new Transition(trade, loadGoods);
            transitionToLoadGoods.triggers.Add(new Trigger_TicksPassed(GenDate.TicksPerHour*4));
            transitionToLoadGoods.preActions.Add(
                new TransitionAction_Message("MessageTraderCaravanLeaving".Translate(faction.Name)));
            transitionToLoadGoods.preActions.Add(new TransitionAction_Custom(() => loadGoods.data = trade.Data));
            mainGraph.transitions.Add(transitionToLoadGoods);

            var leaveColony = new LordToil_ExitMapAndEscortCarriers();
            mainGraph.lordToils.Add(leaveColony);
            var transitionToLeaveColony = new Transition(loadGoods, leaveColony);
            transitionToLeaveColony.triggers.Add(new Trigger_Memo("TravelArrived"));
            mainGraph.transitions.Add(transitionToLeaveColony);

            // attaching defence and flee subgraph
            mainGraph.AttachSubgraph(TraderDefence(mainGraph.lordToils));

            return mainGraph;
        }

        // defence and flee subgraph
        public StateGraph TraderDefence(List<LordToil> sourceLordToils)
        {
            var graph = new StateGraph();

            var toilFlee = new LordToil_TakeWoundedGuest();
            graph.lordToils.Add(toilFlee);

            var toilDefence = new LordToil_DefendCaravan();
            graph.lordToils.Add(toilDefence);

            var transitionToFlee = new Transition(toilDefence, toilFlee);
            transitionToFlee.sources.AddRange(sourceLordToils);
            transitionToFlee.triggers.Add(new Trigger_ImportantCaravanPeopleLost());
            transitionToFlee.preActions.Add(new TransitionAction_Message(
                "MessageVisitorsTakingWounded".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            transitionToFlee.preActions.Add(new TransitionAction_WakeAll());
            graph.transitions.Add(transitionToFlee);

            // for each lordToil added transition to the defence lordToil, and it's toils index is kept to get back to it later
            foreach (var currentToil in sourceLordToils)
            {
                var transitionToDefence = new Transition(currentToil, toilDefence);
                transitionToDefence.triggers.Add(new Trigger_PawnHarmed());
                // keep the toil index that actually triggered transition as reference to return back to
                transitionToDefence.preActions.Add(new TransitionAction_Custom(() =>
                {
                    toilDefence.Data.initialLordToilIndex = sourceLordToils.IndexOf(currentToil);
                }));
                transitionToDefence.preActions.Add(new TransitionAction_WakeAll());
                graph.transitions.Add(transitionToDefence);
            }

            // adds transitions back to the initial lordToil, initialize target toil with temporary null
            var transitionToSource = new Transition(toilDefence, null);
            transitionToSource.triggers.Add(new Trigger_Memo("DefenceSuccessful"));
            // asign target toil to return towith transitionAction
            transitionToSource.preActions.Add(new TransitionAction_Custom(() =>
            {
                transitionToSource.target = sourceLordToils[toilDefence.Data.initialLordToilIndex];
            }));
            graph.transitions.Add(transitionToSource);

            return graph;
        }

        public override void ExposeData()
        {
            Scribe_References.LookReference(ref faction, "faction");
            Scribe_Values.LookValue(ref chillSpot, "chillSpot");
        }
    }
}