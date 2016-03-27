using RimWorld.SquadAI;
using Verse;

namespace RA
{
    public class Trigger_Memo_Individual : Trigger
	{
		public string memo;
        public Pawn pawn;

		public Trigger_Memo_Individual(string memo, Pawn pawn)
		{
			this.memo = memo;
            this.pawn = pawn;
        }

		public override bool ActivateOn(Brain brain, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.Memo && signal.memo == memo && signal.pawn == pawn;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue(ref memo, "memo");
            Scribe_References.LookReference(ref pawn, "pawn");
        }
	}
}
