using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class RA_DefGenerator
    {
        // added new def generators and removed redundant
        public static void GenerateImpliedDefs_PreResolve()
        {
            var defGenerators = ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs().Concat(ThingDefGenerator_Seeds.ImpliedSeedDefs()).Concat(ThingDefGenerator_Corpses.ImpliedCorpseDefs()).Concat(PlaceholdersDefGenerator.ImpliedPlaceholdersDefs().Concat(MinifiedDefGenerator.ImpliedMinifiedDefs()).Concat(UnfinishedDefGenerator.ImpliedUnfinishedDefs()));
            foreach (var thingDef in defGenerators)
            {
                thingDef.PostLoad();
                DefDatabase<ThingDef>.Add(thingDef);
            }

            foreach (var terrainDef in TerrainDefGenerator_Stone.ImpliedTerrainDefs())
            {
                terrainDef.PostLoad();
                DefDatabase<TerrainDef>.Add(terrainDef);
            }

            CrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
        }
    }
}
