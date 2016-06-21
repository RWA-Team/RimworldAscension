using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class RA_WorkGiver_GrowerSow : WorkGiver_GrowerSow
    {
        public override Job JobOnCell(Pawn pawn, IntVec3 cell)
        {
            if (!pawn.CanReserve(cell) || cell.IsForbidden(pawn)) return null;

            DetermineWantedPlantDef(cell);

            // cut improper plants in cell
            var plant = Find.ThingGrid.ThingAt<Plant>(cell);
            if (plant != null)
            {
                if (plant.def != wantedPlantDef && Find.DesignationManager.DesignationOn(plant) == null)
                {
                    DesignatePlantToCut(plant);
                }
                return null;
            }

            // cut improper adjucent sow blockers (trees)
            var adjacentSowBlocker = GenPlant.AdjacentSowBlocker(wantedPlantDef, cell) as Plant;
            if (adjacentSowBlocker != null)
            {
                if (Find.DesignationManager.DesignationOn(adjacentSowBlocker) == null)
                {
                    var zoneGrowing = Find.ZoneManager.ZoneAt(adjacentSowBlocker.Position) as Zone_Growing;
                    if (zoneGrowing == null || zoneGrowing.GetPlantDefToGrow() != adjacentSowBlocker.def)
                    {
                        DesignatePlantToCut(adjacentSowBlocker);
                    }
                }
                return null;
            }
            
            Log.Message("20");
            var haulGarbageAsideJob = HaulGarbageAsideJob(cell, pawn);
            if (haulGarbageAsideJob != null)
            {
                Log.Message("21");
                return haulGarbageAsideJob;
            }

            Log.Message("22");
            if (wantedPlantDef.CanEverPlantAt(cell))
            {
                Log.Message("23");
                return new Job(JobDefOf.Sow, cell)
                {
                    plantDefToSow = wantedPlantDef
                };
            }

            return null;
        }

        public Job HaulGarbageAsideJob(IntVec3 cell, Pawn pawn)
        {
            return Find.ThingGrid.ThingsListAtFast(cell)
                    .Where(thing => thing.def.BlockPlanting && thing.def.EverHaulable)
                    .Select(thing => HaulAIUtility.HaulAsideJobFor(pawn, thing))
                    .FirstOrDefault();
        }

        public void DesignatePlantToCut(Thing thing)
        {
            if (thing.def.plant.IsTree)
                Find.DesignationManager.AddDesignation(new Designation(thing,
                    DefDatabase<DesignationDef>.GetNamedSilentFail("ChopWood")));
            else if ((thing as Plant).HarvestableNow)
                Find.DesignationManager.AddDesignation(new Designation(thing, DesignationDefOf.HarvestPlant));
            else Find.DesignationManager.AddDesignation(new Designation(thing, DesignationDefOf.CutPlant));
            Log.Message("12");
        }
    }
}