using Verse;

namespace RA
{
    public class WorkGiver_CollectSand : WorkGiver_Collect
    {
        public WorkGiver_CollectSand()
        {
            designationDef = DefDatabase<DesignationDef>.GetNamed("CollectSand");
            jobDef = DefDatabase<JobDef>.GetNamed("CollectSand");
        }
    }
}