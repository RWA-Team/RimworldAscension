using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RA
{
    public class Trigger_CanTrade : Trigger
    {
        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame% GenTicks.TickRareInterval == 0)
            {
                var tradeCenters =
                    Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>()
                        .Where(building => building.trader == null);
                var trader = TraderCaravanUtility.FindTrader(lord);
                var carrier = lord.ownedPawns.FirstOrDefault(pawn => pawn.GetCaravanRole() == TraderCaravanRole.Carrier);

                if (tradeCenters.Any() && trader != null && carrier != null)
                {
                    if (trader.CanReach(tradeCenters.FirstOrDefault(), PathEndMode.InteractionCell, trader.NormalMaxDanger()) &&
                        carrier.CanReach(tradeCenters.FirstOrDefault(), PathEndMode.InteractionCell, trader.NormalMaxDanger()))
                        return true;
                }
            }
            return false;
        }
    }
}