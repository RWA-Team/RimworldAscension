using UnityEngine;
using Verse;

namespace RA
{
    // hands are draw only if any position shift from Vector3.zero is set, like (0, 0, 0.1f)
    public class CompHandsDrawer : ThingComp
    {
        public Vector3 FirstHandPosition => (props as CompHandsDrawer_Properties).firstHandPosition;
        public Vector3 SecondHandPosition => (props as CompHandsDrawer_Properties).secondHandPosition;
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