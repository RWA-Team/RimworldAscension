using RimWorld;
using RimWorld.SquadAI;

namespace RA
{
    public static class UtilityGraphs
    {
        public static StateGraph GraphGroupDefence(params State[] sourceStates)
        {
            var graph = new StateGraph();

            var stateDefence = new State_DefendPoint();
            graph.states.Add(stateDefence);
            
            // adds recursive transition to itself when pawns harmed again
            var transition_ToDefence = new Transition(stateDefence, stateDefence);
            transition_ToDefence.sources.AddRange(sourceStates);
            transition_ToDefence.triggers.Add(new Trigger_PawnHarmed());
            transition_ToDefence.preActions.Add(new TransitionAction_SetDefendLocalGroup());
            graph.transitions.Add(transition_ToDefence);
            
            foreach (var state in sourceStates)
            {
                var transition_ToSource = new Transition(stateDefence, state);
                transition_ToSource.triggers.Add(new Trigger_TicksPassed(GenDate.TicksPerHour));
                transition_ToDefence.preActions.Add(new TransitionAction_RenewDefence());
                graph.transitions.Add(transition_ToSource);
            }
            
            return graph;
        }
    }
}
