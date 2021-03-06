﻿using Verse;

namespace RA
{
    public static class RA_DefGenerator
    {
        // added new def generators and removed redundant
        public static void GenerateDefs()
        {
            var defGenerators = UnfinishedDefGenerator.ImpliedUnfinishedDefs();

            foreach (var thingDef in defGenerators)
            {
                thingDef.PostLoad();
                DefDatabase<ThingDef>.Add(thingDef);
            }

            CrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
        }
    }
}