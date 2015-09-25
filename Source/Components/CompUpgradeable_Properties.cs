using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RimworldAscension.Components
{
    public class CompUpgradeable_Properties: CompProperties
    {
        public List<ResearchProjectDef> researchPrerequisites = new List<ResearchProjectDef>();
        public ThingDef upgradeTargetDef = new ThingDef();
        public float upgradeDiscountMultiplier = 0.5f;

        // Default requirement
        public CompUpgradeable_Properties()
        {
            this.compClass = typeof(CompUpgradeable_Properties);
        }
    }
}
