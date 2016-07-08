using UnityEngine;
using Verse;

namespace RA
{
    public class CompHandsDrawer : ThingComp
    {
        public static Graphic handGraphic = GraphicDatabase.Get<Graphic_Single>("Overlays/Hand", ShaderDatabase.CutoutSkin);
        public Vector3 firstHandPos, rightHandPos;

        public Vector3 DefaultFirstHandPosition => (props as CompHandsDrawer_Properties).firstHandPosition;
        public Vector3 DefaultSecondHandPosition => (props as CompHandsDrawer_Properties).secondHandPosition;

        public override void PostDraw()
        {
            if (DefaultFirstHandPosition != Vector3.zero)
                handGraphic.Draw(parent.DrawPos + DefaultFirstHandPosition, parent.Rotation, parent);
            if (DefaultSecondHandPosition != Vector3.zero)
                handGraphic.Draw(parent.DrawPos + DefaultFirstHandPosition, parent.Rotation, parent);
        }
    }

    public class CompHandsDrawer_Properties : CompProperties
    {
        public Vector3 firstHandPosition = Vector3.zero;
        public Vector3 secondHandPosition = Vector3.zero;

        public CompHandsDrawer_Properties()
        {
            compClass = typeof (CompHandsDrawer);
        }
    }
}
