using Verse;

namespace RA
{
    public class SpecialThingFilterWorker_Tools : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return t.def?.weaponTags?.Contains("Tool") ?? false;
        }

        public override bool AlwaysMatches(ThingDef def)
        {
            return def?.weaponTags?.Contains("Tool") ?? false;
        }
    }
}
