using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class MinifiedDefGenerator
    {
        public static IEnumerable<ThingDef> ImpliedMinifiedDefs()
        {
            foreach (var sourceDef in DefDatabase<ThingDef>.AllDefs
                .Where(def => def.Minifiable
                              && def.minifiedDef.defName == "Placeholder").ToList())
            {
                var newDef = new ThingDef
                {
                    defName = sourceDef.defName + "Minified",
                    label = "minified " + sourceDef.label,
                    description = "Minified " + sourceDef.label + ".",
                    thingClass = typeof (MinifiedThing),
                    category = ThingCategory.Item,
                    altitudeLayer = AltitudeLayer.Item,
                    stackLimit = 1,
                    pathCost = 15,
                    alwaysHaulable = true,
                    drawGUIOverlay = true,
                    useHitPoints = true,
                    selectable = true,
                    thingCategories = new List<ThingCategoryDef>(),
                    stuffCategories = sourceDef.stuffCategories
                };

                // assigns MinifiedThings ThingCategory
                CrossRefLoader.RegisterListWantsCrossRef(newDef.thingCategories, "MinifiedThings");

                newDef.comps.Add(new CompProperties_Forbiddable());

                newDef.SetStatBaseValue(StatDefOf.MaxHitPoints,
                    sourceDef.statBases.StatListContains(StatDefOf.MaxHitPoints) ? sourceDef.BaseMaxHitPoints : 100);
                newDef.SetStatBaseValue(StatDefOf.Flammability,
                    sourceDef.statBases.StatListContains(StatDefOf.Flammability) ? sourceDef.BaseFlammability : 1);
                newDef.SetStatBaseValue(StatDefOf.MarketValue,
                    sourceDef.statBases.StatListContains(StatDefOf.MarketValue) ? sourceDef.BaseMarketValue : 100);
                newDef.SetStatBaseValue(StatDefOf.Beauty,
                    sourceDef.statBases.StatListContains(StatDefOf.Beauty)
                        ? sourceDef.GetStatValueAbstract(StatDefOf.Beauty) - 5
                        : -2);

                // assign new minified def to the source ThingDef
                sourceDef.minifiedDef = newDef;

                yield return newDef;
            }
        }
    }
}