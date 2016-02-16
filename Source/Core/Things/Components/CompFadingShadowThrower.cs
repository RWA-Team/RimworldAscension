using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace RA
{
    class CompFadingShadowThrower : ThingComp
    {
        public override void PostPrintOnto(SectionLayer layer)
        {
            if (this.parent.def.graphicData.shadowData != null)
            {
                // shadow offset is not calculated in PrintShadow by default for some reason
                Printer_Shadow.PrintShadow(layer, this.parent.TrueCenter() + this.parent.def.graphicData.shadowData.offset, this.parent.def.graphicData.shadowData);
            }
        }
    }
}
