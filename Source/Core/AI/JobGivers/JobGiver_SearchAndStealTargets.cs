using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobGiver_SearchAndStealTargets : ThinkNode_JobGiver
    {
        // those values can be assigned in JobGiver_Kidnap properties in duty def
        public bool canBush = true;
        public float targetSearchRadius = 20f;
        public int jobMaxDuration = GenDate.TicksPerHour*8;

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("SearchAndStealTargets"))
            {
                maxNumToCarry = 1,
                expiryInterval = jobMaxDuration,
                checkOverrideOnExpire = true,
                canBash = canBush
            };
        }
    }
}
