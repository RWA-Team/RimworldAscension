using Verse;

namespace RA
{
    class CompBlueprintParams : ThingComp
    {
        public CompBlueprintParams_Properties Properties => (CompBlueprintParams_Properties)props;
    }

    public class CompBlueprintParams_Properties : CompProperties
    {
        public string researchName = default(string);

        // Default requirement
        public CompBlueprintParams_Properties()
        {
            compClass = typeof(CompBlueprintParams);
        }
    }
}