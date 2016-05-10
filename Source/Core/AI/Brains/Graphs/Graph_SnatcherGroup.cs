using RimWorld.SquadAI;

namespace RA
{
    public class Graph_SnatcherGroup
    {
        // main graph contains main subgraphs and transitions between them, which determine the overall group behaviour
        public static StateGraph MainGraph()
        {
            var mainGraph = new StateGraph();
            var snatchState = new State_SnatchGoodsOrPeople();
            mainGraph.states.Add(snatchState);
            // this subgraph add defence state and corresponding triggers to the parameter state
            mainGraph.AttachSubgraph(UtilityGraphs.GraphGroupDefence(snatchState));

            return mainGraph;
        }
    }
}
