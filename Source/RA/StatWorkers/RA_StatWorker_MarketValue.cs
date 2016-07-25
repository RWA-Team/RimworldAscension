using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_StatWorker_MarketValue : StatWorker
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

        public override string GetExplanation(StatRequest req, ToStringNumberSense numberSense)
        {
            if (req.HasThing && req.Thing is Pawn)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(base.GetExplanation(req, numberSense));
                var pawn = req.Thing as Pawn;
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("StatsReport_CharacterQuality".Translate() + ": x" + PriceUtility.PawnQualityPriceFactor(pawn).ToStringPercent());
                return stringBuilder.ToString();
            }
            if (req.Def.statBases.StatListContains(StatDefOf.MarketValue))
            {
                return base.GetExplanation(req, numberSense);
            }
            var stringBuilder2 = new StringBuilder();
            stringBuilder2.AppendLine("StatsReport_MarketValueFromStuffsAndWork".Translate());
            return stringBuilder2.ToString();
        }

        public override bool ShouldShowFor(BuildableDef def)
        {
            var thingDef = def as ThingDef;
            return thingDef != null && (TradeUtility.EverTradeable(thingDef) || thingDef.category == ThingCategory.Building);
        }
    }
}
