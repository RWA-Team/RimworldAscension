using Verse;

namespace RA
{
    public class CompAutoCraft : ThingComp
    {
        public float Productivity => (props as CompAutoCraft_Properties).workPerTick;
    }

    public class CompAutoCraft_Properties : CompProperties
    {
        public float workPerTick = 1f;

        public CompAutoCraft_Properties()
        {
            compClass = typeof(CompAutoCraft);
        }
    }
}