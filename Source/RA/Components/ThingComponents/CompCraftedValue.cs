using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RA
{
    public class CompCraftedValue : ThingComp
    {
        public float productionCost = 0.005f;

        public float ValuePerWorkFactor => (props as CompCraftedValue_Properties).valuePerWorkFactor;

        public float SetProductCost(List<Thing> ingredients, float workAmount)
        {
            var ingredientsCost = ingredients.Sum(ingredient => ingredient.MarketValue*ingredient.stackCount);
            var workCost = workAmount*ValuePerWorkFactor;

            return ingredientsCost + workCost;
        }

        public override void PostExposeData()
        {
            Scribe_Values.LookValue(ref productionCost, "productionCost");
        }
    }

    public class CompCraftedValue_Properties : CompProperties
    {
        public float valuePerWorkFactor = 0.005f;

        public CompCraftedValue_Properties()
        {
            compClass = typeof (CompCraftedValue);
        }
    }
}