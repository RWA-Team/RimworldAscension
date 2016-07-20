using Verse;

namespace RA
{
    public static class RA_GraphicUtility
    {
        //changed inner graphic extraction for minified things
        public static Graphic ExtractInnerGraphicFor(this Graphic outerGraphic, Thing thing) => thing.Graphic;
    }
}
