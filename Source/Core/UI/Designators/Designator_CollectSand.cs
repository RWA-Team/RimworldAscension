using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RA
{
    public class Designator_CollectSand : Designator_Collect
    {
        public Designator_CollectSand()
        {
            defaultLabel = "Collect Sand";
            defaultDesc = "Collect sand from sand tiles";
<<<<<<< HEAD
            icon = RA_Assets.Missing;
=======
            icon = ContentFinder<Texture2D>.Get("Missing");
>>>>>>> origin/Wivex-branch

            designationDef = DefDatabase<DesignationDef>.GetNamed("CollectSand");
            allowedTerrain = new List<TerrainDef>
            {
                DefDatabase<TerrainDef>.GetNamed("Sand")
            };
        }
    }
}
