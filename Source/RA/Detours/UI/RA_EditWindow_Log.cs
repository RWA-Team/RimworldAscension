using UnityEngine;
using Verse;

namespace RA
{
    public class RA_EditWindow_Log : EditWindow_Log
    {
        // changes initial log window size
        public override void PostOpen()
        {
            windowRect.x = 10f;
            windowRect.y = 10f;
            windowRect.height = Screen.height - 10f*2;
            windowRect.width = Mathf.Max(700f, (Screen.width + 10f)/2);
        }
    }
}