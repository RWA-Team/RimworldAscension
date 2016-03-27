using RimWorld;
using Verse;

namespace RA
{
    public class RA_FactionGenerator
    {
        public static void GenerateFactionsIntoCurrentWorld()
        {
            foreach (var current in DefDatabase<FactionDef>.AllDefs)
            {
                for (var j = 0; j < current.requiredCountAtGameStart; j++)
                {
                    var faction = FactionGenerator.NewGeneratedFaction(current);
                    Current.World.factionManager.Add(faction);
                }
            }
        }
    }
}
