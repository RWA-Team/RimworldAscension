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
            // first run check to improve performance
            if (!pawn.CanReserve(cell) || cell.IsForbidden(pawn)) return null;

            // set plant to grow
            DetermineWantedPlantDef(cell);

            // cut improper plants in current cell
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
            
            // haul aside objects that block growing
            var haulGarbageAsideJob = HaulGarbageAsideJob(cell, pawn);
            if (haulGarbageAsideJob != null)
            {
                return haulGarbageAsideJob;
            }

            // cultivate or fertilize soil if needed
            var growingZone = Find.ZoneManager.ZoneAt(cell) as RA_Zone_Growing;
            if (growingZone != null)
            {
                if (growingZone.needsFertilization && cell.GetTerrain().defName.Contains("Cultivated"))
                {
                    return pawn.CanReach(cell, PathEndMode.ClosestTouch, pawn.NormalMaxDanger())
                        ? new Job(DefDatabase<JobDef>.GetNamedSilentFail("Fertilize"), cell)
                        : null;
                }
                if (growingZone.needsCultivation && !cell.GetTerrain().defName.Contains("Cultivated") && !cell.GetTerrain().defName.Contains("Fertilized"))
                {
                    if (!Find.DesignationManager.AllDesignationsAt(cell).Any())
                    {
                        Find.DesignationManager.AddDesignation(new Designation(cell,
                            DefDatabase<DesignationDef>.GetNamedSilentFail("CultivateLand")));
                    }
                    return null;
                }
            }


            // last check for allowed planting
            if (wantedPlantDef.CanEverPlantAt(cell))
            {
                // plant
                return new Job(JobDefOf.Sow, cell)
                {
                    plantDefToSow = wantedPlantDef
                };
            }

            // default exit
            return null;
        }

        public Job HaulGarbageAsideJob(IntVec3 cell, Pawn pawn)
        {
            return Find.ThingGrid.ThingsListAtFast(cell)
                    .Where(thing => thing.def.BlockPlanting && thing.def.EverHaulable)
                    .Select(thing => HaulAIUtility.HaulAsideJobFor(pawn, thing))
                    .FirstOrDefault();
        }

        // determines what cut designator to apply, based on the plant type
        public void DesignatePlantToCut(Thing thing)
        {
            if (thing.def.plant.IsTree)
                Find.DesignationManager.AddDesignation(new Designation(thing,
                    DefDatabase<DesignationDef>.GetNamedSilentFail("ChopWood")));
            else if ((thing as Plant).HarvestableNow)
                Find.DesignationManager.AddDesignation(new Designation(thing, DesignationDefOf.HarvestPlant));
            else Find.DesignationManager.AddDesignation(new Designation(thing, DesignationDefOf.CutPlant));
        }
    }
}