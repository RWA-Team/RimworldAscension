using System.Collections.Generic;
using Verse;

namespace RA
{
    public class Designator_CollectClay : Designator_Collect
    {
        public Designator_CollectClay()
        {
            defaultLabel = "Collect Clay";
            defaultDesc = "Collect clay from mud or shallow water tiles";
            icon = Assets.Missing;

            designationDef = DefDatabase<DesignationDef>.GetNamed("CollectClay");
            allowedTerrain = new List<TerrainDef>
            {
                DefDatabase<TerrainDef>.GetNamed("Mud"),
                DefDatabase<TerrainDef>.GetNamed("SoilRich"),
                DefDatabase<TerrainDef>.GetNamed("WaterShallow")
            };
        }
    }
}
