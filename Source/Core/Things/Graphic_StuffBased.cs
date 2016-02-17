using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Verse;
using RimWorld;

namespace RA
{
    public class Graphic_StuffBased : Graphic_Collection
    {
        public Dictionary<StuffCategoryDef, Graphic> stuffBasedGraphics;

        // determine whach graphic(texture) to show 
        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[0].MatSingle;
            }
        }

        // rearrange all graphics in array to proper positions, based on they stuffCategory, setting all missing graphics as BadGraphic material
        public override void Init(GraphicRequest req)
        {
            base.Init(req);

            // subGraphics - initial graphic set, consists of all textures in the thing texture folder
            stuffBasedGraphics = new Dictionary<StuffCategoryDef, Graphic>(this.subGraphics.Length);

            Graphic graphicCurrent;
            for (int i = 0; i < this.subGraphics.Length; i++)
            {
                graphicCurrent = this.subGraphics[i];

                // stuff category def should be the second part in texture name (Cape_Leathery_Front)
                string[] nameParts = graphicCurrent.MatSingle.name.Split('_');
                string stuffCatName = nameParts[1];

                StuffCategoryDef category = DefDatabase<StuffCategoryDef>.GetNamed(stuffCatName);
                if (category != null)
                    stuffBasedGraphics.Add(category, graphicCurrent);
                else
                    stuffBasedGraphics.Add(category, BaseContent.BadGraphic);
            }
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            if (newColorTwo != Color.white)
            {
                Log.ErrorOnce("Cannot use Graphic_Appearances.GetColoredVersion with a non-white colorTwo.", 9910272);
            }
            return GraphicDatabase.Get<Graphic_StuffBased>(this.path, newShader, this.drawSize, newColor);
        }

        public override Material MatSingleFor(Thing thing)
        {
            if (thing != null && thing.Stuff != null)
            {
                return stuffBasedGraphics[thing.Stuff.stuffProps.categories[0]].MatSingleFor(thing);
            }
            else
            {
                Log.Error(string.Format("No graphic for {0} of {1} category.", thing, thing.Stuff.stuffProps.categories[0]));
                return null;
            }
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
        {
            if (thing != null && thing.Stuff != null)
            {
                stuffBasedGraphics[thing.Stuff.stuffProps.categories[0]].DrawWorker(loc, rot, thingDef, thing);
            }
            else
            {
                Log.Error(string.Format("No graphic for {0} of {1} category.", thing, thing.Stuff.stuffProps.categories[0]));
            }
        }

        public override string ToString()
        {
            return string.Concat(new object[]
            {
                "Appearance(path=",
                this.path,
                ", color=",
                this.color,
                ", colorTwo=unsupported)"
            });
        }
    }
}