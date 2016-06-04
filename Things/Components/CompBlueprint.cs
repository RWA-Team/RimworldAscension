using Verse;

namespace RA
{
    class CompBlueprint : ThingComp
    {
        public CompBlueprint_Properties Properties => (CompBlueprint_Properties)props;
    }

    public class CompBlueprint_Properties : CompProperties
    {
        public string researchName = default(string);

        // Default requirement
        public CompBlueprint_Properties()
        {
            compClass = typeof(CompBlueprint);
        }
    }
}