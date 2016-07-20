using System.Collections.Generic;
using Verse;

namespace RA
{
    public class CompWorktableExtended : ThingComp
    {
        public CompWorktableExtended_Properties Properties => (CompWorktableExtended_Properties)props;
    }

    public class CompWorktableExtended_Properties : CompProperties
    {
        public bool showsIngridients = true;
        public bool autoProcessing = true;
        public List<IntVec3> ingridientCells = new List<IntVec3>();

        // Default requirement
        public CompWorktableExtended_Properties()
        {
            compClass = typeof(CompWorktableExtended);
        }
    }
}