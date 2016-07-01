using RimWorld;
using Verse;

namespace RA
{
    public class StatWorker_HiddenValue : StatWorker
    {
        public override bool ShouldShowFor(BuildableDef def) => false;
    }
}
