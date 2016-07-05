using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_StatWorker_MarketValue : StatWorker_MarketValue
    {
        public const float ValuePerWorkFactor = 0.004f;

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (!req.HasThing)
                return 0f;

            // market value for pawn
            if (req.Thing is Pawn)
            {
                return base.GetValueUnfinalized(StatRequest.For(req.Def, req.StuffDef), applyPostProcess)*
                       PriceUtility.PawnQualityPriceFactor((Pawn) req.Thing);
            }

            // market value for buildings
            if (req.Thing is Building)
            {
                // market value from costList
                var marketValueFromResources = 0f;
                if (!req.Def.costList.NullOrEmpty())
                {
                    marketValueFromResources +=
                        req.Def.costList.Sum(thingCount => thingCount.count * thingCount.thingDef.BaseMarketValue);
                }

                // market value from costStuffCount
                var marketValueFromStuff = 0f;
                if (req.StuffDef != null && req.Def.costStuffCount > 0)
                {
                    marketValueFromStuff += req.Def.costStuffCount / req.StuffDef.VolumePerUnit *
                                            req.StuffDef.BaseMarketValue;
                }

                // market value from workToMake
                var marketValueFromWork = req.Def.GetStatValueAbstract(StatDefOf.WorkToMake, req.StuffDef) *
                                          ValuePerWorkFactor;

                return marketValueFromResources + marketValueFromStuff + marketValueFromWork;
            }

            // market value for things
            var marketValue = 0f;

            // market value is taken from baseStats, if MarketValue value is set there
            if (req.Def.statBases.StatListContains(StatDefOf.MarketValue))
            {
                // base market value
                marketValue = base.GetValueUnfinalized(req);
            }

            // ingredients value
            var compCraftedValue = req.Thing.TryGetComp<CompCraftedValue>();
            if (compCraftedValue != null)
                marketValue += compCraftedValue.productionCost;
            else
            {
                var recipe = DefDatabase<RecipeDef>.GetNamed("Make" + req.Thing.def.defName);
                foreach (var ingredient in recipe.ingredients)
                {
                    if (ingredient.filter.AllowedThingDefs.Any(def => def.IsStuff
                                                                      && req.Thing.def.stuffProps.CanMake(def)))
                        marketValue += req.Thing.Stuff.BaseMarketValue *
                                       ingredient.GetBaseCount();
                    else
                        marketValue += ingredient.filter.AllowedThingDefs.Average(def => def.BaseMarketValue) *
                                       ingredient.GetBaseCount();
                }
            }

            return marketValue;
        }
    }
}
