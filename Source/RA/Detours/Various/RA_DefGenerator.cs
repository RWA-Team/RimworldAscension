using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RA
{
    public static class RA_DefGenerator
    {
        public static void GenerateImpliedDefs_PreResolve()
        {
            var defGenerators = ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs().Concat(ThingDefGenerator_Seeds.ImpliedSeedDefs()).Concat(ThingDefGenerator_Corpses.ImpliedCorpseDefs());

            foreach (var current in defGenerators)
            {
                current.PostLoad();
                DefDatabase<ThingDef>.Add(current);
            }
            CrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
            //foreach (var current2 in TerrainDefGenerator_Stone.ImpliedTerrainDefs())
            //{
            //    current2.PostLoad();
            //    DefDatabase<TerrainDef>.Add(current2);
            //}
        }
    }
}
