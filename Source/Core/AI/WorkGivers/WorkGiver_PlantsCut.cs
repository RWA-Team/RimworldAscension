using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_PlantsCut : WorkGiver_WorkWithTools
    {
        //used to keep current tool equipped if there are available unfinished jobs for this tool type
        public static bool hasPotentialJobs;

        public WorkGiver_PlantsCut()
        {
            workTypeName = "PlantCutting";
        }

        public Job ActualJob(Thing target)
        {
            // if designation is to harvest plant
            if (Find.DesignationManager.DesignationOn(target).def == DesignationDefOf.HarvestPlant)
            {
                if (!((Plant)target).HarvestableNow)
                {
                    return null;
                }
                return new Job(JobDefOf.Harvest, target);
            }
            // designation is to cut plant
            else
            {
                return new Job(JobDefOf.CutPlant, target);
            }
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // search things throught designations is faster than searching designations through all things
            // all things marked for plantcutting or harvesting
            IEnumerable<Thing> designatedTargets = Find.DesignationManager.allDesignations.FindAll(designation => designation.def == DesignationDefOf.CutPlant || designation.def == DesignationDefOf.HarvestPlant).Select(designation => designation.target.Thing);

            IEnumerable<Thing> availableTargets = designatedTargets.Where(target => pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()) && !target.IsBurning());

            hasPotentialJobs = availableTargets.Count() > 0 ? true : false;

            return DoJobWithTool(pawn, availableTargets, ActualJob);
        }
    }
}
