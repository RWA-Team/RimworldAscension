using System.Collections.Generic;
using System.Linq;
using System;
using RimWorld;
using Verse;

namespace RA
{
    public class CompCraftedValue : ThingComp
    {
        public const float BalanceValue = 1.4f;

        public float ValuePerWork => (props as CompCraftedValue_Properties).valuePerWorkFactor;
        public float ProfitFactor => (props as CompCraftedValue_Properties).profitFactor;

        public float SetProductCost(RecipeDef recipe, List<Thing> currentIngredients)
        {
            var workValue = parent.def.GetStatValueAbstract(StatDefOf.WorkToMake)*ValuePerWork;

            var curIngredientsValue = currentIngredients.Sum(ingredient => ingredient.MarketValue*ingredient.stackCount);
            var minIngredientsValue = recipe.ingredients.Sum(ingredient =>
                ingredient.filter.AllowedThingDefs.Min(def => def.BaseMarketValue)*ingredient.GetBaseCount());
            var maxIngredientsValue = recipe.ingredients.Sum(ingredient =>
                ingredient.filter.AllowedThingDefs.Max(def => def.BaseMarketValue)*ingredient.GetBaseCount());

            var minMarketValue = workValue + minIngredientsValue;
            var maxMarketValue = workValue + maxIngredientsValue;

            var B = maxIngredientsValue != minIngredientsValue
                ? (maxMarketValue - minMarketValue)/
                  (float) Math.Pow(maxIngredientsValue - minIngredientsValue, ProfitFactor)*(BalanceValue/workValue)
                : 0f;

            var marketValue = B*(float) Math.Pow(curIngredientsValue - minIngredientsValue, ProfitFactor) +
                              workValue + minIngredientsValue;

            return marketValue;
        }
    }

    public class CompCraftedValue_Properties : CompProperties
    {
        public float valuePerWorkFactor = 0.005f;
        public float profitFactor = 1.2f;

        public CompCraftedValue_Properties()
        {
            compClass = typeof (CompCraftedValue);
        }
    }
}