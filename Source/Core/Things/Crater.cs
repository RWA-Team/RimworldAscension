using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace RA
{
    public class Crater : ThingWithComps
    {
        public float innerRadius;

        // change drawsize of the crater if specific radius is set
        public override Graphic Graphic
        {
            get
            {
                if (innerRadius == 0)
                {
                    return def.graphic;
                }
                else
                {
                    return GraphicDatabase.Get<Graphic_Single>(def.graphic.path, def.graphic.Shader, new Vector2(def.graphic.drawSize.x * innerRadius, def.graphic.drawSize.y * innerRadius), def.graphic.Color);
                }
            }
        }

        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();

            // Save tickstoimpact to save file
            Scribe_Values.LookValue<float>(ref innerRadius, "radius");
        }
    }
}