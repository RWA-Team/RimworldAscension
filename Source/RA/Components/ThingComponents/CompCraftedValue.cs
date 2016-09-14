using System.Collections.Generic;
using System.Linq;
using System;
using RimWorld;
using Verse;

namespace RA
{
    public class CompCraftedValue : ThingComp
    {
        public float marketValue = -1;

        public float ValuePerWork => (props as CompCraftedValue_Properties).valuePerWorkFactor;
        public float ProfitFactor => (props as CompCraftedValue_Properties).profitFactor;

        // set market value from the actual ingredients costs
        public void SetMarketValue(RecipeDef recipe, List<Thing> currentIngredients)
        {
            var workValue = parent.def.GetStatValueAbstract(StatDefOf.WorkToMake) * ValuePerWork;

            var curIngredientsValue = currentIngredients.Sum(ingredient => ingredient.MarketValue * ingredient.stackCount);
            var minIngredientsValue = recipe.ingredients.Sum(ingredient =>
                ingredient.filter.AllowedThingDefs.Min(def => def.BaseMarketValue) * ingredient.GetBaseCount());
            var maxIngredientsValue = recipe.ingredients.Sum(ingredient =>
                ingredient.filter.AllowedThingDefs.Max(def => def.BaseMarketValue) * ingredient.GetBaseCount());

            var profitCoefficient = maxIngredientsValue != minIngredientsValue
                ? (float)Math.Pow(maxIngredientsValue - minIngredientsValue, 1 - ProfitFactor)
                : 0f;

            marketValue = profitCoefficient * (curIngredientsValue - minIngredientsValue) + workValue + minIngredientsValue;
        }

        // predict possible market value for spawned items
        public float PredictMarketValue()
        {
            var recipe = DefDatabase<RecipeDef>.GetNamed("Make" + parent.def.defName);

            var workValue = parent.def.GetStatValueAbstract(StatDefOf.WorkToMake) * ValuePerWork;

            var curIngredientsValue = 0f;
            foreach (var ingredient in recipe.ingredients)
            {
                if (parent.Stuff != null && ingredient.filter.AllowedThingDefs.Contains(parent.Stuff))
                {
                    // if ingredient is stuff based, choose it's stuff as known ingredient here
                    curIngredientsValue += parent.Stuff.BaseMarketValue * ingredient.GetBaseCount();
                }
                else
                    curIngredientsValue += ingredient.filter.AllowedThingDefs.RandomElement().BaseMarketValue * ingredient.GetBaseCount();
            }

            var minIngredientsValue = recipe.ingredients.Sum(ingredient =>
                ingredient.filter.AllowedThingDefs.Min(def => def.BaseMarketValue) * ingredient.GetBaseCount());
            var maxIngredientsValue = recipe.ingredients.Sum(ingredient =>
                ingredient.filter.AllowedThingDefs.Max(def => def.BaseMarketValue) * ingredient.GetBaseCount());

            var profitCoefficient = maxIngredientsValue != minIngredientsValue
                ? (float)Math.Pow(maxIngredientsValue - minIngredientsValue, 1 - ProfitFactor)
                : 0f;

            marketValue = profitCoefficient * (curIngredientsValue - minIngredientsValue) + workValue + minIngredientsValue;

            return marketValue;
        }

        // predict possible market value for spawned items
        public static float PredictBaseMarketValue(ThingDef thingDef, CompCraftedValue_Properties compProps)
        {
            var recipe = DefDatabase<RecipeDef>.GetNamed("Make" + thingDef.defName);

            var workValue = thingDef.GetStatValueAbstract(StatDefOf.WorkToMake) * compProps.valuePerWorkFactor;

            var randomIngredientsValue = recipe.ingredients.Sum(ingredient => ingredient.filter.AllowedThingDefs.RandomElement().BaseMarketValue * ingredient.GetBaseCount());

            var minIngredientsValue = recipe.ingredients.Sum(ingredient =>
                ingredient.filter.AllowedThingDefs.Min(def => def.BaseMarketValue) * ingredient.GetBaseCount());
            var maxIngredientsValue = recipe.ingredients.Sum(ingredient =>
                ingredient.filter.AllowedThingDefs.Max(def => def.BaseMarketValue) * ingredient.GetBaseCount());

            var profitCoefficient = maxIngredientsValue != minIngredientsValue
                ? (float)Math.Pow(maxIngredientsValue - minIngredientsValue, 1 - compProps.profitFactor)
                : 0f;

            return profitCoefficient * (randomIngredientsValue - minIngredientsValue) + workValue + minIngredientsValue;
        }
    }

    public class CompCraftedValue_Properties : CompProperties
    {
        public float valuePerWorkFactor = 0.005f;
        public float profitFactor = 1.2f;

        public CompCraftedValue_Properties()
        {
            compClass = typeof(CompCraftedValue);
        }
    }
}