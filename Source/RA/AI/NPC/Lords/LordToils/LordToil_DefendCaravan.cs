using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RA
{
    public class LordToil_DefendCaravan : LordToil
    {
        public const int DefaultCombatTimer = GenTicks.TickRareInterval;

        public LordToil_DefendCaravan()
        {
            data = new LordToilData_CaravanDefence(GenTicks.TickLongInterval);
        }

        public LordToilData_CaravanDefence Data => (LordToilData_CaravanDefence)data;
        public override IntVec3 FlagLoc => TraderCaravanUtility.FindTrader(lord).Position;
        //public override bool AllowSatisfyLongNeeds => true;
        
        public override void Init()
        {
            var trader = TraderCaravanUtility.FindTrader(lord);
            if (trader != null && trader.mindState.wantsToTradeWithColony)
                trader.mindState.wantsToTradeWithColony = false;
        }

        public override void UpdateAllDuties()
        {
            var trader = TraderCaravanUtility.FindTrader(lord);
            if (trader != null)
            {
                trader.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Station"));
                foreach (var pawn in lord.ownedPawns)
                {
                    switch (pawn.GetCaravanRole())
                    {
                        case TraderCaravanRole.Carrier:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Follow, trader, 5f);
                            break;
                        case TraderCaravanRole.Guard:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, trader.Position, 20);
                            break;
                        case TraderCaravanRole.Chattel:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Escort, trader, 8f);
                            break;
                    }
                }
            }
        }

        // renew combat timer
        public void Notify_PawnTookDamage(Pawn victim, DamageInfo dinfo)
        {
            Data.combatTimer = DefaultCombatTimer;
        }

        public override void LordToilTick()
        {
            if (Data.combatTimer-- <= 0)
            {
                lord.ReceiveMemo("DefenceSuccessful");
            }
        }
    }
}