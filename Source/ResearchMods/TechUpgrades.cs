using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RimworldAscension
{
    public static class TechUpgrades
    {
        public static void ToMedieval()
        {
            FactionDef colony = DefDatabase<FactionDef>.GetNamed("Colony", true);
            colony.techLevel = TechLevel.Medieval;
        }

    }
}