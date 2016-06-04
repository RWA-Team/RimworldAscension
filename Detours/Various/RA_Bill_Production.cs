using RimWorld;
using Verse;

namespace RA
{
    public class RA_Bill_Production : Bill_Production
    {
        public override void Notify_IterationCompleted(Pawn billDoer)
        {
            // skip message prompt for research recipes
            if (recipe.jobString == "Doing research.") return;

            if (repeatMode == BillRepeatMode.RepeatCount)
            {
                repeatCount--;
                if (repeatCount == 0)
                {
                    Messages.Message("MessageBillComplete".Translate(LabelCap), (Thing)billStack.billGiver, MessageSound.Benefit);
                }
            }
        }
    }
}
