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
}
