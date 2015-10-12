using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.SquadAI;

namespace RimworldAscension
{
    public class GraphMaker_Trader
    {
        public static StateGraph TradeGraph(Building_TradingPost tradingPost)
        {
            IntVec3 departureCell;
            CellFinder.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(IsValidEdgeCell), out departureCell);
            departureCell = CellFinder.RandomClosewalkCellNear(departureCell, 3);

            // States determine the pawn's behaviour for each period of incident
            StateGraph stateGraph = new StateGraph();
            // AttachSubgraph adds multiple states in one subgraph to the current main graph
            State stateArrival = stateGraph.AttachSubgraph(ArrivalGraph(tradingPost));
            State_Trading stateTrading = new State_Trading(tradingPost);
            stateGraph.states.Add(stateTrading);
            State_GatherCaravan stateGathering = new State_GatherCaravan(tradingPost);
            stateGraph.states.Add(stateGathering);
            State stateDeparture = stateGraph.AttachSubgraph(DepartureGraph(departureCell));

            // Transitions determine the sequence of states, which change due to the corresponding trigger prerequisites
            Transition transitionArrivalToTrading = new Transition(stateArrival, stateTrading);
            transitionArrivalToTrading.triggers.Add(new Trigger_Memo("TravelArrived"));
            stateGraph.transitions.Add(transitionArrivalToTrading);

            Transition transitionTradingToGathering = new Transition(stateTrading, stateGathering);
            // 7500 ticks ~ 6 game hours
            transitionTradingToGathering.triggers.Add(new Trigger_TicksPassed(7500));
            transitionTradingToGathering.preActions.Add(new TransitionAction_Message("Traders Gathering to Depart"));
            stateGraph.transitions.Add(transitionTradingToGathering);

            Transition transitionGatheringToDeparture = new Transition(stateGathering, stateDeparture);
            transitionGatheringToDeparture.triggers.Add(new Trigger_TicksPassed(500));
            transitionGatheringToDeparture.triggers.Add(new Trigger_Memo("CaravanGathered"));
            transitionGatheringToDeparture.preActions.Add(new TransitionAction_Message("Traders Leaving"));
            stateGraph.transitions.Add(transitionGatheringToDeparture);

            return stateGraph;
        }

        public static StateGraph ArrivalGraph(Building_TradingPost tradingPost)
        {
            // Work Graph
            StateGraph stateGraph = new StateGraph();

            // State traveling to target point
            State_Travel stateTravel = new State_Travel(tradingPost);
            stateGraph.states.Add(stateTravel);

            // State defend point
            State_DefendPoint stateDefendPoint = new State_DefendPoint();
            stateGraph.states.Add(stateDefendPoint);

            // State flee
            State_PanicFlee stateTravelFlee = new State_PanicFlee();
            stateGraph.states.Add(stateTravelFlee);

            // Trigger: Pawn harmed while walking: defend yourself
            Transition transitionAttackedDefend = new Transition(stateTravel, stateDefendPoint);
            transitionAttackedDefend.triggers.Add(new Trigger_PawnHarmed());
            transitionAttackedDefend.preActions.Add(new TransitionAction_SetDefendLocalGroup());
            stateGraph.transitions.Add(transitionAttackedDefend);

            // Trigger: defend time passed, resume traveling
            Transition transitionContinueTravel = new Transition(stateDefendPoint, stateTravel);
            // NOTE: why so much time to recover from fight?
            transitionContinueTravel.triggers.Add(new Trigger_TicksPassed(4000));
            // adds goodwill with trader's faction if defend is success
            transitionContinueTravel.preActions.Add(new TransitionAction_TraderDefendSuccess());
            stateGraph.transitions.Add(transitionContinueTravel);

            // Trigger: Pawn harmed while defending: renew defend point
            Transition transitionRenewDefendPoint = new Transition(stateDefendPoint, stateDefendPoint);
            transitionRenewDefendPoint.triggers.Add(new Trigger_PawnHarmed_NotIfPawnHarmedFlee());
            stateGraph.transitions.Add(transitionRenewDefendPoint);

            // Trigger: groups: % pawns lost => flee / single: harmed => flee
            Transition transitionFlee = new Transition(stateDefendPoint, stateTravelFlee);
            transitionFlee.triggers.Add(new Trigger_PawnHarmedFlee(0.5f));
            transitionFlee.preActions.Add(new TransitionAction_TraderHarmedFlee());
            stateGraph.transitions.Add(transitionFlee);

            return stateGraph;
        }

        public static StateGraph DepartureGraph(IntVec3 travelDest)
        {
            // Work Graph
            StateGraph stateGraph = new StateGraph();

            // State traveling to target point
            State_TravelAndExit stateTravel = new State_TravelAndExit(travelDest);
            stateGraph.states.Add(stateTravel);
            //stateGraph.StartingState = stateTravel;

            // State defend point
            State_DefendPoint stateDefendPoint = new State_DefendPoint();
            stateGraph.states.Add(stateDefendPoint);

            // State flee
            State_PanicFlee stateTravelFlee = new State_PanicFlee();
            stateGraph.states.Add(stateTravelFlee);

            // Trigger: Pawn harmed while walking: defend yourself
            Transition transitionAttackedDefend = new Transition(stateTravel, stateDefendPoint);
            transitionAttackedDefend.triggers.Add(new Trigger_PawnHarmed());
            transitionAttackedDefend.preActions.Add(new TransitionAction_SetDefendLocalGroup());
            stateGraph.transitions.Add(transitionAttackedDefend);

            // Trigger: defend time passed, resume traveling
            Transition transitionContinueTravel = new Transition(stateDefendPoint, stateTravel);
            transitionContinueTravel.triggers.Add(new Trigger_TicksPassed(4000));
            stateGraph.transitions.Add(transitionContinueTravel);

            // Trigger: Pawn harmed while defending: renew defend point
            Transition transitionRenewDefendPoint = new Transition(stateDefendPoint, stateDefendPoint);
            transitionRenewDefendPoint.triggers.Add(new Trigger_PawnHarmed_NotIfPawnHarmedFlee());
            stateGraph.transitions.Add(transitionRenewDefendPoint);

            // Trigger: groups: % pawns lost => flee / single: harmed => flee
            Transition transitionFlee = new Transition(stateDefendPoint, stateTravelFlee);
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
