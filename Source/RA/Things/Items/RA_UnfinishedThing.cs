using UnityEngine;
using Verse;

namespace RA
{
    public class RA_UnfinishedThing : UnfinishedThing
    {
        public static Graphic unfinishedOverlayGraphic;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (unfinishedOverlayGraphic == null)
            {
                unfinishedOverlayGraphic = GraphicDatabase.Get<Graphic_Single>("Overlays/UnfinishedBorder",
                    ShaderDatabase.Transparent, new Vector2(RotatedSize.x + 0.1f, RotatedSize.z + 0.1f),
                    new Color(1, 1, 1, 0.5f));
                //unfinishedOverlayGraphic = GraphicDatabase.Get<Graphic_Single>("UI/Icons/UnfinishedIndicator", ShaderDatabase.Transparent, new Vector2(0.65f, 0.65f), new Color(1, 1, 1, 0.7f));
            }
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            base.DrawAt(drawLoc);
            unfinishedOverlayGraphic.Draw(drawLoc + new Vector3(0, -0.001f, 0), Rot4.North, this);
        }
    }
}