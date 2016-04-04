using System.Collections.Generic;
using Verse;

namespace RA
{
    public class Designator_CollectSand : Designator_Collect
    {
        public Designator_CollectSand()
        {
            defaultLabel = "Collect Sand";
            defaultDesc = "Collect sand from sand tiles";
            designationIconPath = "Missing";

            designationDef = DefDatabase<DesignationDef>.GetNamed("CollectSand");
            allowedTerrain = new List<TerrainDef>
            {
                DefDatabase<TerrainDef>.GetNamed("Sand")
            };
        }
    }
}
