using RimWorld;
using Verse;

namespace RA
{
    public class RA_StatWorker_MarketValue : StatWorker_MarketValue
    {
        public static bool once = false;
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            // market value for crafted things
            if (req.HasThing)
            {
                var compCraftedValue = req.Thing?.TryGetComp<CompCraftedValue>();
                if (compCraftedValue != null)
                {
                    return compCraftedValue.marketValue != -1
                        ? compCraftedValue.marketValue
                        : compCraftedValue.PredictMarketValue();
                }
            }
            else if (Current.ProgramState != ProgramState.Entry)
            {
                var def = req.Def as ThingDef;
                var compProps = def?.CompDefFor<CompCraftedValue>() as CompCraftedValue_Properties;
                if (compProps != null)
                    return CompCraftedValue.PredictBaseMarketValue(def, compProps);
            }
            return base.GetValueUnfinalized(req, applyPostProcess);
        }
    }
}