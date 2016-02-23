using System;
using System.Collections;

using UnityEngine;
using Verse;

namespace RA
{
    public class Graphic_StuffBased : Graphic_Appearances
    {
        public override void Init(GraphicRequest req)
        {
            base.Init(req);
            Graphic[] array = new Graphic[this.subGraphics.Length];
            for (int i = 0; i < this.subGraphics.Length; i++)
            {
                array[i] = this.subGraphics[i];
            }
            this.subGraphics = new Graphic[Enum.GetNames(typeof(StuffAppearance)).Length];
            IEnumerator enumerator = Enum.GetValues(typeof(StuffAppearance)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    StuffAppearance stuffAppearance = (StuffAppearance)((byte)enumerator.Current);
                    Graphic graphic = BaseContent.BadGraphic;
                    for (int j = 0; j < array.Length; j++)
                    {
                        Graphic graphic2 = array[j];
                        string[] array2 = graphic2.MatSingle.name.Split(new char[]
                        {
                            '_'
                        });
                        string a = array2[array2.Length - 1];
                        if (a == stuffAppearance.ToString())
                        {
                            graphic = graphic2;
                            break;
                        }
                        if (graphic == null && a == StuffAppearance.Smooth.ToString())
                        {
                            graphic = graphic2;
                        }
                    }
                    this.subGraphics[(int)stuffAppearance] = graphic;
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            if (newColorTwo != Color.white)
            {
                Log.ErrorOnce("Cannot use Graphic_Appearances.GetColoredVersion with a non-white colorTwo.", 9910251);
            }
            return GraphicDatabase.Get<Graphic_Appearances>(this.path, newShader, this.drawSize, newColor);
        }

        public override Material MatSingleFor(Thing thing)
        {
            StuffAppearance stuffAppearance = StuffAppearance.Smooth;
            if (thing != null && thing.Stuff != null)
            {
                stuffAppearance = thing.Stuff.stuffProps.appearance;
            }
            Graphic graphic = this.subGraphics[(int)stuffAppearance];
            return graphic.MatSingleFor(thing);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
        {
            StuffAppearance stuffAppearance = StuffAppearance.Smooth;
            if (thing != null && thing.Stuff != null)
            {
                stuffAppearance = thing.Stuff.stuffProps.appearance;
            }
            Graphic graphic = this.subGraphics[(int)stuffAppearance];
            graphic.DrawWorker(loc, rot, thingDef, thing);
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