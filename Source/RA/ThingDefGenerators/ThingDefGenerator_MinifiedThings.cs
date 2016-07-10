using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class ThingDefGenerator_MinifiedThings
    {
        public static IEnumerable<ThingDef> ImpliedMinifiedDefs()
        {
            foreach (var sourceDef in DefDatabase<ThingDef>.AllDefs.ToList())
            {
                if (!sourceDef.Minifiable)
                    continue;

                var newDef = new ThingDef
                {
                    resourceReadoutPriority = ResourceCountPriority.Middle,
                    category = ThingCategory.Item,
                    thingClass = typeof (MinifiedThing),
                    graphicData = new GraphicData {graphicClass = typeof (Graphic_Single)},
                    useHitPoints = true,
                    selectable = true
                };
                
                // inherited values from source
                newDef.SetStatBaseValue(StatDefOf.MaxHitPoints, sourceDef.BaseMaxHitPoints);
                newDef.SetStatBaseValue(StatDefOf.Flammability, sourceDef.BaseFlammability);
                newDef.SetStatBaseValue(StatDefOf.MarketValue, sourceDef.BaseMarketValue);
                
                // any item values
                newDef.altitudeLayer = AltitudeLayer.Item;
                newDef.stackLimit = 1;
                newDef.comps.Add(new CompProperties_Forbiddable());
                newDef.SetStatBaseValue(StatDefOf.Beauty, -5);
                newDef.alwaysHaulable = true;
                newDef.drawGUIOverlay = true;
                newDef.rotatable = false;
                newDef.pathCost = 15;

                //Data used for all leathers
                newDef.category = ThingCategory.Item;
                newDef.description = "LeatherDesc".Translate(sourceDef.label);
                newDef.useHitPoints = true;
                newDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 100);
                newDef.SetStatBaseValue(StatDefOf.MarketValue, sourceDef.race.leatherMarketValue);
                if (newDef.thingCategories == null)
                    newDef.thingCategories = new List<ThingCategoryDef>();
                CrossRefLoader.RegisterListWantsCrossRef(newDef.thingCategories, "Leathers");
                newDef.graphicData.texPath = "Things/Item/Resource/Cloth";
                newDef.stuffProps = new StuffProperties();
                CrossRefLoader.RegisterListWantsCrossRef(newDef.stuffProps.categories, "Leathery");

                StatUtility.SetStatValueInList(ref newDef.stuffProps.statFactors, StatDefOf.MarketValue, 1.3f);
                StatUtility.SetStatValueInList(ref newDef.stuffProps.statFactors, StatDefOf.ArmorRating_Blunt, 1.5f);
                StatUtility.SetStatValueInList(ref newDef.stuffProps.statFactors, StatDefOf.ArmorRating_Sharp, 1.5f);
                StatUtility.SetStatValueInList(ref newDef.stuffProps.statFactors, StatDefOf.ArmorRating_Heat, 1.7f);
                StatUtility.SetStatValueInList(ref newDef.stuffProps.statFactors, StatDefOf.ArmorRating_Electric, 4.0f);


                //Copy relevant properties into leather def
                newDef.defName = sourceDef.defName + "_Leather";
                if (!sourceDef.race.leatherLabel.NullOrEmpty())
                    newDef.label = sourceDef.race.leatherLabel;
                else
                    newDef.label = "LeatherLabel".Translate(sourceDef.label);
                newDef.stuffProps.color = sourceDef.race.leatherColor;
                newDef.graphicData.color = sourceDef.race.leatherColor;
                newDef.graphicData.colorTwo = sourceDef.race.leatherColor;
                //newDef.stuffProps.stuffCommonality = eachLeatherCommonality*sourceDef.race.leatherCommonalityFactor;
                StatUtility.SetStatValueInList(ref newDef.stuffProps.statFactors, StatDefOf.Insulation_Cold,
                    sourceDef.race.leatherInsulation);
                StatUtility.SetStatValueInList(ref newDef.stuffProps.statFactors, StatDefOf.Insulation_Heat,
                    sourceDef.race.leatherInsulation);


                // Stat factors
                var sfos = sourceDef.race.leatherStatFactors;
                if (sfos != null)
                {
                    foreach (var fo in sfos)
                    {
                        StatUtility.SetStatValueInList(ref newDef.stuffProps.statFactors, fo.stat, fo.value);
                    }
                }

                //Link race def to this leather def
                sourceDef.race.leatherDef = newDef;

                yield return newDef;
            }
        }
    }
}