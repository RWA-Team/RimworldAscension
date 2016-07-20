using System.Collections.Generic;
using Verse;

namespace RA
{
    public static class PlaceholdersDefGenerator
    {
        public static IEnumerable<ThingDef> ImpliedPlaceholdersDefs()
        {
            var newDef = new ThingDef
            {
                defName = "Human_Meat",
                label = "hm",
                description = "",
                category = ThingCategory.Item,
                thingClass = typeof (ThingWithComps),
                graphicData = new GraphicData { graphicClass = typeof(Graphic_Single) },
                menuHidden = true,
                destroyOnDrop = true
            };
            newDef.graphicData.texPath = "Missing";

            yield return newDef;
        }
    }
}