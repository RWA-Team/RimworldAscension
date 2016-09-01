using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public static class UnfinishedDefGenerator
    {
        public static IEnumerable<ThingDef> ImpliedUnfinishedDefs()
        {
            foreach (var sourceDef in DefDatabase<RecipeDef>.AllDefs.Where(def =>
                def.UsesUnfinishedThing && def.unfinishedThingDef.defName == "UnfinishedThing").ToList())
            {
                var firstProductDef = sourceDef.products.FirstOrDefault().thingDef;

                var newDef = new ThingDef
                {
                    defName = firstProductDef.defName + "Unfinished",
                    label = "unfinished " + firstProductDef.label,
                    description = "Unfinished " + firstProductDef.label + ".",
                    thingClass = typeof (RA_UnfinishedThing),
                    category = ThingCategory.Item,
                    altitudeLayer = AltitudeLayer.Item,
                    stackLimit = 1,
                    pathCost = 15,
                    alwaysHaulable = true,
                    drawGUIOverlay = true,
                    useHitPoints = true,
                    selectable = true,
                    isUnfinishedThing = true,
                    thingCategories = new List<ThingCategoryDef>(),
                    stuffCategories = firstProductDef.stuffCategories,
                    graphicData = new GraphicData()
                };

                newDef.graphicData.CopyFrom(firstProductDef.graphicData);
                newDef.graphicData.drawSize = Vector2.one;

                // assigns MinifiedThings ThingCategory
                CrossRefLoader.RegisterListWantsCrossRef(newDef.thingCategories, "UnfinishedThings");

                newDef.comps.Add(new CompProperties_Forbiddable());

                newDef.SetStatBaseValue(StatDefOf.MaxHitPoints,
                    firstProductDef.statBases.StatListContains(StatDefOf.MaxHitPoints)
                        ? firstProductDef.BaseMaxHitPoints
                        : 100);
                newDef.SetStatBaseValue(StatDefOf.Flammability,
                    firstProductDef.statBases.StatListContains(StatDefOf.Flammability)
                        ? firstProductDef.BaseFlammability
                        : 1);
                newDef.SetStatBaseValue(StatDefOf.MarketValue,
                    firstProductDef.statBases.StatListContains(StatDefOf.MarketValue)
                        ? firstProductDef.BaseMarketValue
                        : 100);
                newDef.SetStatBaseValue(StatDefOf.Beauty,
                    firstProductDef.statBases.StatListContains(StatDefOf.Beauty)
                        ? firstProductDef.GetStatValueAbstract(StatDefOf.Beauty) - 5
                        : -2);

                // assign new minified def to the source ThingDef
                sourceDef.unfinishedThingDef = newDef;

                yield return newDef;
            }
        }
    }
}