using Verse;

namespace RA
{
    public class WorkGiver_CollectClay : WorkGiver_Collect
    {
        public WorkGiver_CollectClay()
        {
            designationDef = DefDatabase<DesignationDef>.GetNamed("CollectClay");
            jobDef = DefDatabase<JobDef>.GetNamed("CollectClay");
        }
    }
}