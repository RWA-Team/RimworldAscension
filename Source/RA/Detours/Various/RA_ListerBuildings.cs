using System.Linq;
using Verse;

namespace RA
{
    class RA_ListerBuildings
    {
        public bool ColonistsHaveResearchBench()
        {
            return Find.ListerBuildings.allBuildingsColonist.Any(building => building.TryGetComp<CompResearcher>() != null);
        }
    }
}
