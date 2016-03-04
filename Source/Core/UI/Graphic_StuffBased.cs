﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using Verse;

namespace RA
{
    public enum StuffCategory : byte
    {
        Woody,
        Stony
    }

    public class Graphic_StuffBased : Graphic
    {
        public Dictionary<string, Graphic> categorizedGraphics = new Dictionary<string, Graphic>();
        public string currentCategory = default(string);

        public override void Init(GraphicRequest req)
        {
            path = req.path;
            drawSize = req.drawSize;
            color = req.color;
            colorTwo = req.colorTwo;

            var basePath = req.path;
            foreach (var category in Enum.GetNames(typeof(StuffCategory)))
            {
                req.path = basePath + "/" + category;
                var textures = ContentFinder<Texture2D>.GetAllInFolder(req.path).ToList();
                if (!textures.NullOrEmpty())
                {
                    var textureNameParts = textures[0].name.Split('_');
                    var graphic = DetermineGraphicType(textureNameParts[textureNameParts.Length - 1]);

                    if (graphic is Graphic_Single)
                    {
                        req.path += "/" + textures[0].name;
                    }

                    graphic.Init(req);
                    categorizedGraphics.Add(category, graphic);
                }
            }
        }

        public Graphic DetermineGraphicType(string textureNameEnding)
        {
            // any that ends with _side/_back/_front or thier mask versions
            if (Regex.IsMatch(textureNameEnding, @"_(front|back|side)[m]?$"))
                return new Graphic_Multi();
            // any that ends with _a/_b/.../_l or thier mask versions
            if (Regex.IsMatch(textureNameEnding, @"_[a-l][m]?$"))
                return new Graphic_StackCount();
            // any that ends with _A/_B/.../_L or thier mask versions
            if (Regex.IsMatch(textureNameEnding, @"_[A-L][m]?$"))
                return new Graphic_Random();

            return new Graphic_Single();
        }

        // called by draw method of the thing, initial draw entry. Determine actual graphic instance to draw, with proper texture
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
        {
            GraphicClassSelector(thing).DrawWorker(loc, rot, thingDef, thing);
        }

        public override Material MatSingleFor(Thing thing)
        {
            return GraphicClassSelector(thing).MatSingleFor(thing);
        }

        public override Material MatSingle => currentCategory != default(string)
            ? categorizedGraphics[currentCategory].MatSingle
            : UncategorisedGraphic.MatSingle;

        public Graphic GraphicClassSelector(Thing thing)
        {
            if (thing?.Stuff != null)
            {
                var category = thing.Stuff.stuffProps.categories[0].defName;
                return categorizedGraphics[category];
            }

            return currentCategory != default(string)
                ? categorizedGraphics[currentCategory]
                : UncategorisedGraphic;
        }

        public Graphic GraphicClassSelector()
        {
            return GraphicClassSelector(null);
        }

        public Graphic UncategorisedGraphic => categorizedGraphics.Values.First();

        // called when no category specified
        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicClassSelector().GetColoredVersion(newShader, newColor, newColorTwo);
        }

        public override string ToString()
        {
            return string.Concat("Appearance(path=", path, ", color=", color, ", colorTwo=unsupported)");
        }
    }
}