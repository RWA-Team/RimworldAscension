using RimWorld.SquadAI;
using Verse;

namespace RA
{
    public class ThievesGroupAI
    {
        // main graph contains main subgraphs and transitions between them, which determine the overall group behaviour
        public static StateGraph MainGraph()
        {
            // States determine the pawn's behaviour for each period of incident
            var AIGraph = new StateGraph();
            // AttachSubgraph adds multiple states in one subgraph to the current main graph
            var stateArrival = AIGraph.AttachSubgraph(ArrivalGraph(tradingPost));
            var stateTrading = new State_Trading(tradingPost);
            AIGraph.states.Add(stateTrading);
            var stateGathering = new State_GatherCaravan(tradingPost);
            AIGraph.states.Add(stateGathering);
            var stateDeparture = AIGraph.AttachSubgraph(DepartureGraph(departureCell));

            // Transitions determine the sequence of states, which change due to the corresponding trigger prerequisites
            var transitionArrivalToTrading = new Transition(stateArrival, stateTrading);
            transitionArrivalToTrading.triggers.Add(new Trigger_Memo("TravelArrived"));
            AIGraph.transitions.Add(transitionArrivalToTrading);

            var transitionTradingToGathering = new Transition(stateTrading, stateGathering);
            // 7500 ticks ~ 6 game hours
            transitionTradingToGathering.triggers.Add(new Trigger_TicksPassed(7500));
            transitionTradingToGathering.preActions.Add(new TransitionAction_Message("Traders Gathering to Depart"));
            AIGraph.transitions.Add(transitionTradingToGathering);

            var transitionGatheringToDeparture = new Transition(stateGathering, stateDeparture);
            transitionGatheringToDeparture.triggers.Add(new Trigger_TicksPassed(500));
            transitionGatheringToDeparture.triggers.Add(new Trigger_Memo("CaravanGathered"));
            transitionGatheringToDeparture.preActions.Add(new TransitionAction_Message("Traders Leaving"));
            AIGraph.transitions.Add(transitionGatheringToDeparture);

            return AIGraph;
        }

        public static StateGraph SnatchersAI()
        {
            // Work Graph
            var AIGraph = new StateGraph();



            // State traveling to target point
            var stateSnatchTarget = new State_Travel(tradingPost);
            AIGraph.states.Add(stateSnatchTarget);

            // State defend point
            var stateDefendPoint = new State_DefendPoint();
            AIGraph.states.Add(stateDefendPoint);

            // State flee
            var stateTravelFlee = new State_PanicFlee();
            AIGraph.states.Add(stateTravelFlee);

            // Trigger: Pawn harmed while walking: defend yourself
            var transitionAttackedDefend = new Transition(stateSnatchTarget, stateDefendPoint);
            transitionAttackedDefend.triggers.Add(new Trigger_PawnHarmed());
            transitionAttackedDefend.preActions.Add(new TransitionAction_SetDefendLocalGroup());
            AIGraph.transitions.Add(transitionAttackedDefend);

            // Trigger: defend time passed, resume traveling
            var transitionContinueTravel = new Transition(stateDefendPoint, stateSnatchTarget);
            // NOTE: why so much time to recover from fight?
            transitionContinueTravel.triggers.Add(new Trigger_TicksPassed(4000));
            // adds goodwill with trader's faction if defend is success
            transitionContinueTravel.preActions.Add(new TransitionAction_TraderDefendSuccess());
            AIGraph.transitions.Add(transitionContinueTravel);

            // Trigger: Pawn harmed while defending: renew defend point
            var transitionRenewDefendPoint = new Transition(stateDefendPoint, stateDefendPoint);
            transitionRenewDefendPoint.triggers.Add(new Trigger_PawnHarmed_NotIfPawnHarmedFlee());
            AIGraph.transitions.Add(transitionRenewDefendPoint);

            // Trigger: groups: % pawns lost => flee / single: harmed => flee
            var transitionFlee = new Transition(stateDefendPoint, stateTravelFlee);
            transitionFlee.triggers.Add(new Trigger_PawnHarmedFlee(0.5f));
            transitionFlee.preActions.Add(new TransitionAction_TraderHarmedFlee());
            AIGraph.transitions.Add(transitionFlee);

            return AIGraph;
        }

        public static StateGraph DepartureGraph(IntVec3 travelDest)
        {
            // Work Graph
            var stateGraph = new StateGraph();

            // State traveling to target point
            var stateTravel = new State_TravelAndExit(travelDest);
            stateGraph.states.Add(stateTravel);
            //stateGraph.StartingState = stateTravel;

            // State defend point
            var stateDefendPoint = new State_DefendPoint();
            stateGraph.states.Add(stateDefendPoint);

            // State flee
            var stateTravelFlee = new State_PanicFlee();
            stateGraph.states.Add(stateTravelFlee);

            // Trigger: Pawn harmed while walking: defend yourself
            var transitionAttackedDefend = new Transition(stateTravel, stateDefendPoint);
            transitionAttackedDefend.triggers.Add(new Trigger_PawnHarmed());
            transitionAttackedDefend.preActions.Add(new TransitionAction_SetDefendLocalGroup());
            stateGraph.transitions.Add(transitionAttackedDefend);

            // Trigger: defend time passed, resume traveling
            var transitionContinueTravel = new Transition(stateDefendPoint, stateTravel);
            transitionContinueTravel.triggers.Add(new Trigger_TicksPassed(4000));
            stateGraph.transitions.Add(transitionContinueTravel);

            // Trigger: Pawn harmed while defending: renew defend point
            var transitionRenewDefendPoint = new Transition(stateDefendPoint, stateDefendPoint);
            transitionRenewDefendPoint.triggers.Add(new Trigger_PawnHarmed_NotIfPawnHarmedFlee());
            stateGraph.transitions.Add(transitionRenewDefendPoint);

            // Trigger: groups: % pawns lost => flee / single: harmed => flee
            var transitionFlee = new Transition(stateDefendPoint, stateTravelFlee);
            transitionFlee.triggers.Add(new Trigger_PawnHarmedFlee(0.5f));
            transitionFlee.preActions.Add(new TransitionAction_TraderHarmedFlee());
            stateGraph.transitions.Add(transitionFlee);

            return stateGraph;
        }

        public static bool IsValidEdgeCell(IntVec3 cell)
        {
            if (!cell.InBounds() || !cell.Walkable())
                return false;

            return true;
        }
    }
}
