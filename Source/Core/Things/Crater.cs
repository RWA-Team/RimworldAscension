﻿using System.Collections.Generic;

using Verse;
using Verse.Sound;
using UnityEngine;
using RimWorld;

namespace RA
{
    public class Crater : ThingWithComps
    {
        public float impactRadius;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.Position, impactRadius, true))
            {
                // set open terrain underneath to gravel
                Thing building = cell.GetThingList().Find(thing => thing.def.category == ThingCategory.Building);
                if (building == null)
                {
                    Find.TerrainGrid.SetTerrain(cell, DefDatabase<TerrainDef>.GetNamed("Gravel"));
                }

            }
        }

        // change drawsize of the crater if specific radius is set
        public override Graphic Graphic
        {
            get
            {
                if (impactRadius == 0)
                {
                    return def.graphic;
                }
                else
                {
                    return GraphicDatabase.Get<Graphic_Single>(def.graphic.path, def.graphic.Shader, def.graphicData.drawSize * impactRadius * 2, def.graphic.Color);
                }
            }
        }

        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();

            // Save tickstoimpact to save file
            Scribe_Values.LookValue(ref impactRadius, "impactRadius");
        }
    }
}