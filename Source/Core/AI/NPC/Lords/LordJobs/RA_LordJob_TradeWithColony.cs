using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RA
{
    // all Triggers are single use and need to initialized for each case of application

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

            var waitAtChillSpot = new RA_LordToil_DefendTraderCaravan();
            mainGraph.lordToils.Add(waitAtChillSpot);
            var transitionToChill = new Transition(arriveAtChillSpot, waitAtChillSpot);
            transitionToChill.triggers.Add(new Trigger_Memo("TravelArrived"));
            mainGraph.transitions.Add(transitionToChill);

            var unloadGoods = new LordToil_UnloadGoods();
            mainGraph.lordToils.Add(unloadGoods);
            var transitionToUnloadGoods = new Transition(waitAtChillSpot, unloadGoods);
            transitionToUnloadGoods.triggers.Add(new Trigger_CanTrade());
            mainGraph.transitions.Add(transitionToUnloadGoods);

            var trade = new LordToil_Trade();
            mainGraph.lordToils.Add(trade);
            var transitionToTrade = new Transition(unloadGoods, trade);
            transitionToTrade.triggers.Add(new Trigger_Memo("TravelArrived"));
            transitionToTrade.preActions.Add(new TransitionAction_TransferTradeData());
            mainGraph.transitions.Add(transitionToTrade);

            var loadGoods = new LordToil_LoadGoods();
            mainGraph.lordToils.Add(loadGoods);
            var transitionToLoadGoods = new Transition(trade, loadGoods);
            transitionToLoadGoods.triggers.Add(new Trigger_TicksPassed(GenDate.TicksPerHour*6));
            transitionToLoadGoods.preActions.Add(
                new TransitionAction_Message("MessageTraderCaravanLeaving".Translate(faction.name)));
            transitionToLoadGoods.preActions.Add(new TransitionAction_TransferTradeData());
            mainGraph.transitions.Add(transitionToLoadGoods);

            var leaveColony = new LordToil_ExitMapAndEscortCarriers();
            mainGraph.lordToils.Add(leaveColony);
            var transitionToLeaveColony = new Transition(loadGoods, leaveColony);
            transitionToLeaveColony.triggers.Add(new Trigger_Memo("TravelArrived"));
            transitionToLeaveColony.preActions.Add(new TransitionAction_TransferTradeData());
            mainGraph.transitions.Add(transitionToLeaveColony);

            // attaching defence and flee subgraph
            mainGraph.AttachSubgraph(TraderDefence(mainGraph.lordToils));

            return mainGraph;
        }

        // defence and flee subgraph
        public StateGraph TraderDefence(List<LordToil> sourceLordToils)
        {
            var graph = new StateGraph();
            var lordDefence = new RA_LordToil_DefendTraderCaravan();
            graph.lordToils.Add(lordDefence);
            // TODO: check if works properly without breaking current shooting or chosen targets
            // adds recursive transition to itself when pawns harmed again
            var transitionToDefence = new Transition(lordDefence, lordDefence);
            transitionToDefence.sources.AddRange(sourceLordToils);
            transitionToDefence.triggers.Add(new Trigger_PawnHarmed());
            graph.transitions.Add(transitionToDefence);

            // adds transitions to flee, rescueing wounded
            var toilFlee = new LordToil_TakeWoundedGuest();
            graph.lordToils.Add(toilFlee);
            var transitionToFlee = new Transition(lordDefence, toilFlee);
            transitionToFlee.sources.AddRange(sourceLordToils);
            transitionToFlee.triggers.Add(new Trigger_ImportantCaravanPeopleLost());
            transitionToFlee.preActions.Add(
                new TransitionAction_Message(
                    "MessageVisitorsTakingWounded".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.name)));
            graph.transitions.Add(transitionToFlee);

            // adds transitions back to the current sourceToil
            foreach (var lordToil in sourceLordToils)
            {
                var transitionToSource = new Transition(lordDefence, lordToil);
                transitionToSource.triggers.Add(new Trigger_TicksPassed(GenDate.TicksPerHour));
                graph.transitions.Add(transitionToSource);
            }

            return graph;
        }

        public override void ExposeData()
        {
            Scribe_References.LookReference(ref faction, "faction");
            Scribe_Values.LookValue(ref chillSpot, "chillSpot");
        }
    }
}