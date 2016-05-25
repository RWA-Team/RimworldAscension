using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace RA.CR
{
    public class CompProperties_Inventory : CompProperties
    {
        public CompProperties_Inventory()
        {
            this.compClass = typeof(CompInventory);
        }
    }
}
