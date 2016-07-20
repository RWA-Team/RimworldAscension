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
            // market value for pawn
            if (req.HasThing && req.Thing is Pawn)
            {
                return base.GetValueUnfinalized(StatRequest.For(req.Def, req.StuffDef), applyPostProcess)*
                       PriceUtility.PawnQualityPriceFactor((Pawn) req.Thing);
            }

            // market value for crafted things
            var compCraftedValue = req.Thing.TryGetComp<CompCraftedValue>();
            if (compCraftedValue != null)
                return compCraftedValue.marketValue;

            // market value by base value
            if (req.Def.statBases.StatListContains(StatDefOf.MarketValue))
            {
                return base.GetValueUnfinalized(req);
            }

            // market value from costList
            var marketValueFromResources = 0f;
            if (!req.Def.costList.NullOrEmpty())
            {
                marketValueFromResources +=
                    req.Def.costList.Sum(thingCount => thingCount.count*thingCount.thingDef.BaseMarketValue);
            }

            // market value from costStuffCount
            var marketValueFromStuff = 0f;
            if (req.StuffDef != null && req.Def.costStuffCount > 0)
            {
                marketValueFromStuff += req.Def.costStuffCount/req.StuffDef.VolumePerUnit*
                                        req.StuffDef.BaseMarketValue;
            }

            // market value from workToMake
            var marketValueFromWork = req.Def.GetStatValueAbstract(StatDefOf.WorkToMake, req.StuffDef)*
                                      ValuePerWorkFactor;

            return marketValueFromResources + marketValueFromStuff + marketValueFromWork;
        }
    }
}
