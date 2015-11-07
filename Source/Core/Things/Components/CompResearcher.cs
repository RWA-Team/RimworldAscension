using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace RA
{
    public class CompResearcher : ThingComp
    {
        public CompResearcher_Properties Properties
        {
            get
            {
                return (CompResearcher_Properties)props;
            }
        }
    }

    public class CompResearcher_Properties : CompProperties
    {
        public Dictionary<ResearchProjectDef, RecipeDef> researchRecipes = new Dictionary<ResearchProjectDef, RecipeDef>();

        // Default requirement
        public CompResearcher_Properties()
        {
            this.compClass = typeof(CompResearcher_Properties);
        }
    }
}
