using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using Verse;

namespace RA
{
    /// texture path should be equal to "/.../Table", where Table folder contains folders for each possible stuff type used for the table,
    /// being named as that stuff category like: "/.../Table/Stony", "/.../Table/Woody", ...
    /// Any new added stuff categories should be added to the StuffCategory enuerator above
    public class Graphic_StuffBased : Graphic
    {
        public Dictionary<string, Graphic> categorizedGraphics = new Dictionary<string, Graphic>();
        public string currentCategory = default(string);

        // TODO: add some way to keep track of used textures after game loads (it gets reset to MatSingle or first stuff from list)

        public override void Init(GraphicRequest req)
        {
            path = req.path;
            drawSize = req.drawSize;
            color = req.color;
            colorTwo = req.colorTwo;
            
            foreach (var directory in ContentFinder<DirectoryInfo>.GetAllInFolder(req.path))
            {
                path = directory.FullName;
                var textures = ContentFinder<Texture2D>.GetAllInFolder(directory.FullName).ToList();
                if (!textures.NullOrEmpty())
                {
                    var textureNameParts = textures[0].name.Split('_');
                    var graphic = DetermineGraphicType(textureNameParts[textureNameParts.Length - 1]);

                    if (graphic is Graphic_Single)
                    {
                        req.path += "/" + textures[0].name;
                    }

                    graphic.Init(req);
                    categorizedGraphics.Add(directory.Name, graphic);
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