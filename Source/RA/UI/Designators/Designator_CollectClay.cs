using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RA
{
    public class Designator_CollectClay : Designator_Collect
    {
        public Designator_CollectClay()
        {
            defaultLabel = "Collect Clay";
            defaultDesc = "Collect clay from mud, rich soil or shallow water tiles";
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                icon = ContentFinder<Texture2D>.Get("UI/Gizmoes/CollectClay");
            });

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
