using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_StatWorker_MarketValue : StatWorker_MarketValue
    {
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            // market value for crafted things
            var compCraftedValue = req.Thing.TryGetComp<CompCraftedValue>();
            if (compCraftedValue != null)
            {
                return compCraftedValue.marketValue != -1
                    ? compCraftedValue.marketValue
                    : compCraftedValue.PredictMarketValue();
            }
            return base.GetValueUnfinalized(req, applyPostProcess);
        }
    }
}