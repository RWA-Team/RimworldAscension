using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RA
{
    public class Drug : ThingWithComps
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            Action action = () =>
            {
                var job = new Job(DefDatabase<JobDef>.GetNamed("TakeDrug"), this);
                pawn.drafter.TakeOrderedJob(job);
            };
            yield return new FloatMenuOption("Start using " + Label, action);
        }
    }
}