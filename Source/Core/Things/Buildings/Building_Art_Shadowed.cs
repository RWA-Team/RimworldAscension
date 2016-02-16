using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;

namespace RA
{
    public class Building_Art_Shadowed : Building_Art
    {
        public override void Print(SectionLayer layer)
        {
            base.Print(layer);

            if (this.def.graphicData.shadowData != null)
            {
                // shadow offset is not calculated in PrintShadow by default for some reason
                Printer_Shadow.PrintShadow(layer, this.TrueCenter() + this.def.graphicData.shadowData.offset, this.def.graphicData.shadowData);
            }
        }
    }
}
