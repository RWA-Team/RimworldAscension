using RimWorld;
using Verse;

namespace RA
{
    public class RA_FactionGenerator
    {
        public static void GenerateFactionsIntoCurrentWorld()
        {
            foreach (FactionDef current in DefDatabase<FactionDef>.AllDefs)
            {
                for (int j = 0; j < current.requiredCountAtGameStart; j++)
                {
                    Faction faction = FactionGenerator.NewGeneratedFaction(current);
                    Current.World.factionManager.Add(faction);
                }
            }
        }
    }
}
