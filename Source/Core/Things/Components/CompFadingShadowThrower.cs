using Verse;

namespace RA
{
    class CompFadingShadowThrower : ThingComp
    {
        public override void PostPrintOnto(SectionLayer layer)
        {
            if (parent.def.graphicData.shadowData != null)
            {
                // shadow offset is not calculated in PrintShadow by default for some reason
                Printer_Shadow.PrintShadow(layer, parent.TrueCenter() + parent.def.graphicData.shadowData.offset, parent.def.graphicData.shadowData);
            }
        }
    }
}
