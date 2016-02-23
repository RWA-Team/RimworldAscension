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
        Dictionary<StuffCategoryDef, Graphic> categorizedGraphics;

        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[0].MatSingle;
            }
        }

        // NOTE: require disposing, as vanilla does?
        public override void Init(GraphicRequest req)
        {
            base.Init(req);

            if (Game.Mode == GameMode.MapPlaying)
            {
                categorizedGraphics = new Dictionary<StuffCategoryDef, Graphic>(this.subGraphics.Length);

                // subGraphics - initial graphic set, consistsof all textures in the thing folder
                foreach (Graphic graphic in this.subGraphics)
                {
                    string[] textureNameParts = graphic.MatSingle.name.Split('_');
                    // stuff category name should be the first after def name in texture name (to avoid problem)
                    // it has index 2 here, because index 0 equal to shader type (added by game engine)
                    string stuffName = textureNameParts[2];

                    StuffCategoryDef def = DefDatabase<StuffCategoryDef>.GetNamed(stuffName);
                    if (def != null)
                        categorizedGraphics.Add(def, graphic);
                    else
                        categorizedGraphics.Add(def, BaseContent.BadGraphic);
                }
            }
        }

        public override Material MatSingleFor(Thing thing)
        {
            if (thing != null && thing.Stuff != null)
            {
                StuffCategoryDef category = thing.Stuff.stuffProps.categories[0];
                // calls MatSingleFor() for the corresponding Graphic class (not this one)
                return categorizedGraphics[category].MatSingleFor(thing);
            }
            else
            {
                Graphic graphic = this.subGraphics[0];
                return graphic.MatSingleFor(thing);
            }
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
        {
            if (thing != null && thing.Stuff != null)
            {
                StuffCategoryDef category = thing.Stuff.stuffProps.categories[0];
                Log.Message("category " + category);
                Log.Message("value " + categorizedGraphics[category]);
                // calls MatSingleFor() for the corresponding Graphic class (not this one)
                categorizedGraphics[category].DrawWorker(loc, rot, thingDef, thing);
            }
            // draw stuffless graphic
            else
            {
                Log.Message("STUFFLESS");
                Graphic graphic = this.subGraphics[0];
                graphic.DrawWorker(loc, rot, thingDef, thing);
            }
        }

        // THIS SHOULD CHOOSE GRAPHIC
        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            Log.Message("COLORED");
            if (newColorTwo != Color.white)
            {
                Log.ErrorOnce("Cannot use Graphic_StuffBased.GetColoredVersion with a non-white colorTwo.", 9910252);
            }
            return GraphicDatabase.Get<Graphic_StuffBased>(this.path, newShader, this.drawSize, newColor);
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