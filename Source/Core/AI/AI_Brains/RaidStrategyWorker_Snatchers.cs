using RimWorld;
using RimWorld.SquadAI;
using Verse;

namespace RA
{
    class RaidStrategyWorker_Snatchers : RaidStrategyWorker
    {
        public override StateGraph MakeBrainGraph(ref IncidentParms parms)
        {
            return ThievesGroupAI.MainGraph();
        }
    }
}
