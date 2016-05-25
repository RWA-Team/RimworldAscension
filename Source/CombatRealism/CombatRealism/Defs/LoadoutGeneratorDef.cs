using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace RA.CR
{
    public class LoadoutGeneratorDef : ThingDef
    {
        public LoadoutGenerator loadoutGenerator;
        public int priority = 0;
    }
}
