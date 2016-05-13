using RA;
using RimWorld;
using Verse;
using Verse.AI.Group;

public class LordToilData_Trade : LordToilData
{
    public TradeCenter selectedTradeCenter;
    public TraderKindDef traderKindDef;
    public Pawn trader, carrier;
    public IntVec3 traderDest, carrierDest;

    public override void ExposeData()
    {
        Scribe_References.LookReference(ref selectedTradeCenter, "selectedTradeCenter");
        Scribe_Defs.LookDef(ref traderKindDef, "traderKindDef");
        Scribe_References.LookReference(ref trader, "trader");
        Scribe_References.LookReference(ref carrier, "carrier");
        Scribe_Values.LookValue(ref traderDest, "traderDest");
        Scribe_Values.LookValue(ref carrierDest, "carrierDest");
    }
}