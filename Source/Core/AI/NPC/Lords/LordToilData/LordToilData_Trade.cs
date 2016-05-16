using RA;
using Verse;
using Verse.AI.Group;

namespace RA
{
    public class LordToilData_Trade : LordToilData
    {
        public TradeCenter selectedTradeCenter;
        public Pawn trader, carrier;
        public IntVec3 traderDest, carrierDest;

        public override void ExposeData()
        {
            Scribe_References.LookReference(ref selectedTradeCenter, "selectedTradeCenter");
            Scribe_References.LookReference(ref trader, "trader");
            Scribe_References.LookReference(ref carrier, "carrier");
            Scribe_Values.LookValue(ref traderDest, "traderDest");
            Scribe_Values.LookValue(ref carrierDest, "carrierDest");
        }
    }
}