using UnityEngine;
using Verse;

namespace RA
{
    [StaticConstructorOnStartup]
    public class RA_UnfinishedThing: UnfinishedThing
    {
        public static Graphic crateFrontGraphic = GraphicDatabase.Get<Graphic_Single>("UI/Icons/UnfinishedBorder", ShaderDatabase.Transparent, new Vector2(1.1f, 1.1f), new Color(1, 1, 1, 0.5f));
        //public static Graphic crateFrontGraphic = GraphicDatabase.Get<Graphic_Single>("UI/Icons/UnfinishedIndicator", ShaderDatabase.Transparent, new Vector2(0.65f, 0.65f), new Color(1, 1, 1, 0.7f));

        public override void DrawAt(Vector3 drawLoc)
        {
            base.DrawAt(drawLoc);
            crateFrontGraphic.Draw(drawLoc + new Vector3(0, -0.001f, 0), Rot4.North, this);
        }
    }
}