using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    [StaticConstructorOnStartup]
    public class RA_UI_BackgroundMain : UI_BackgroundMain
    {
        public static Vector2 MainBackgroundSize = new Vector2(2000f, 1190f);
        public static Texture2D MainMenuBackground = ContentFinder<Texture2D>.Get("UI/MainMenu/Background");

        public override void BackgroundOnGUI()
        {
            var flag = !(Screen.width > Screen.height*(MainBackgroundSize.x/MainBackgroundSize.y));
            Rect position;
            if (flag)
            {
                var height = Screen.height;
                var num = Screen.height*(MainBackgroundSize.x/MainBackgroundSize.y);
                position = new Rect(Screen.width/2 - num/2f, 0f, num, height);
            }
            else
            {
                var width = Screen.width;
                var num = Screen.width*(MainBackgroundSize.y/MainBackgroundSize.x);
                position = new Rect(0f, Screen.height/2 - num/2f, width, num);
            }

            GUI.DrawTexture(position, MainMenuBackground, ScaleMode.ScaleToFit);
        }
    }
}
