using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class CompContainer_Properties : CompProperties
    {
        // Default value
        public int itemsCap = 10;

        // Default requirement
        public CompContainer_Properties()
        {
            this.compClass = typeof(CompContainer_Properties);
        }
    }
}
