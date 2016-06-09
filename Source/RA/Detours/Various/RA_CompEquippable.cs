using Verse;
using Verse.AI;

namespace RA
{
    // interrupt current job if pawn drops tool while doing it
    public class RA_CompEquippable : CompEquippable
    {
        public new void Notify_Dropped()
        {
            foreach (var verb in AllVerbs)
            {
                verb.Notify_Dropped();
            }

            // tools injection
            var compTool = parent.TryGetComp<CompTool>();
            if (compTool != null) PrimaryVerb.CasterPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
    }
}
