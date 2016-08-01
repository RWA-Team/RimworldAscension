using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_Bill_Production : Bill_Production
    {
        // skip message prompt for research recipes
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            if (recipe.jobString == "Doing research.") return;

            base.Notify_IterationCompleted(billDoer, ingredients);
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
