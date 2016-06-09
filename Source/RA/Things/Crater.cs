
using UnityEngine;
using Verse;

namespace RA
{
    public class Crater : ThingWithComps
    {
        public float impactRadius;
        public int craterNumber;

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            foreach (var cell in GenRadial.RadialCellsAround(Position, impactRadius, true))
            {
                // set open terrain underneath to gravel
                var building = cell.GetThingList().Find(thing => thing.def.category == ThingCategory.Building);
                if (building == null)
                {
                    Find.TerrainGrid.SetTerrain(cell, DefDatabase<TerrainDef>.GetNamed("Gravel"));
                }
            }

            craterNumber = Find.ListerThings.ThingsOfDef(def).Count;
        }

        public override Vector3 DrawPos => base.DrawPos + new Vector3(0, craterNumber/1000f, 0);

        // change drawsize of the crater if specific radius is set
        public override Graphic Graphic
        {
            get
            {
                if (impactRadius == 0)
                {
                    return def.graphic;
                }
                return GraphicDatabase.Get<Graphic_Single>(def.graphic.path, def.graphic.Shader, def.graphicData.drawSize * impactRadius * 2, def.graphic.Color);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.LookValue(ref impactRadius, "impactRadius");
            Scribe_Values.LookValue(ref craterNumber, "craterNumber");
        }
    }
}