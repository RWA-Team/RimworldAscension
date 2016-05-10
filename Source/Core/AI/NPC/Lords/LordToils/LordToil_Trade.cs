using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RA
{
    public class LordToil_Trade : LordToil
    {
        public LordToilData_Trade Data => (LordToilData_Trade)data;
        public override IntVec3 FlagLoc => Data.selectedTradeCenter.InteractionCell;
        public override bool AllowSatisfyLongNeeds => false;

        public override void Init()
        {
            Data.selectedTradeCenter.trader = Data.trader;
            // transfer all carrier inventory to the trade center
            while (Data.carrier.inventory.container.Count > 0)
            {
                var transferedThing = Data.carrier.inventory.container.FirstOrDefault();
                Data.carrier.inventory.container.TransferToContainer(transferedThing, Data.selectedTradeCenter.traderStock,
                    transferedThing.stackCount);
            }
            Data.trader.mindState.wantsToTradeWithColony = true;

            // find close, but not adjucent to the trade cell for carrier to station at
            IntVec3 destination;
            CellFinder.TryFindRandomReachableCellNear(Data.selectedTradeCenter.InteractionCell, 4,
                TraverseParms.For(Data.carrier),
                dest => !FindUtil.SquareAreaAround(dest, 3).Contains(dest), region => true,
                out destination);
            Data.carrierDest = destination;
        }

        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns)
            {
                if (pawn == Data.trader)
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Station"));
                else if (pawn == Data.carrier)
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("StationAt"), Data.carrierDest);
                else
                {
                    switch (pawn.GetCaravanRole())
                    {
                        case TraderCaravanRole.Guard:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Escort, Data.trader, 10f)
                            {
                                locomotion = LocomotionUrgency.Amble
                            };
                            break;
                        default:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Follow, Data.trader, 5f)
                            {
                                locomotion = LocomotionUrgency.Amble
                            };
                            break;
                    }
                }
            }
        }
    }
}