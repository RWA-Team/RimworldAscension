using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RA
{
    public class LordToil_UnloadGoods : LordToil
    {
        public LordToilData_Trade Data => (LordToilData_Trade)data;
        public override IntVec3 FlagLoc => Data.selectedTradeCenter.InteractionCell;
        public override bool AllowSatisfyLongNeeds => false;

        public override void Init()
        {
            data = new LordToilData_Trade();
            Data.selectedTradeCenter = Find.ListerBuildings.AllBuildingsColonistOfClass<TradeCenter>().FirstOrDefault(building => building.trader == null);
            Data.trader = lord.ownedPawns.FirstOrDefault(pawn => pawn.GetCaravanRole() == TraderCaravanRole.Trader);
            Data.carrier = lord.ownedPawns.FirstOrDefault(pawn => pawn.GetCaravanRole() == TraderCaravanRole.Carrier);
            Data.traderDest = Data.selectedTradeCenter.InteractionCell;
            Data.carrierDest = Data.selectedTradeCenter.InteractionCell + new IntVec3(0, 0, -1);
        }

        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns)
            {
                if (pawn == Data.trader)
                {
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("StationAt"), Data.traderDest)
                    {
                        locomotion = LocomotionUrgency.Walk
                    };
                }
                else if (pawn == Data.carrier)
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("StationAt"), Data.carrierDest)
                    {
                        locomotion = LocomotionUrgency.Walk
                    };
                else
                {
                    switch (pawn.GetCaravanRole())
                    {
                        case TraderCaravanRole.Guard:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Escort, Data.trader, 5f)
                            {
                                locomotion = LocomotionUrgency.Walk
                            };
                            break;
                        default:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Follow, Data.trader, 8f)
                            {
                                locomotion = LocomotionUrgency.Walk
                            };
                            break;
                    }
                }
            }
        }

        // should check for state completion conditions and actions
        public override void LordToilTick()
        {
            if (Find.TickManager.TicksGame%205 == 0)
            {
                if (Data.trader.Position == Data.traderDest && Data.carrier.Position == Data.carrierDest)
                {
                    lord.ReceiveMemo("TravelArrived");
                }
            }
        }
    }
}
