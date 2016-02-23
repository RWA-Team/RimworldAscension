using UnityEngine;
using Verse;

namespace RA
{
    public class RA_UI_BackgroundMain : UIMenuBackground
    {
        public static readonly Texture2D MainBackground = ContentFinder<Texture2D>.Get("UI/MainMenu/Background", true);
        public static readonly Vector2 MainBackgroundSize = new Vector2(2000f, 1190f);

        public override void BackgroundOnGUI()
        {
            bool flag = true;
            if (Screen.width > Screen.height * (MainBackgroundSize.x / MainBackgroundSize.y))
            {
                flag = false;
            }
            Rect position;
            if (flag)
            {
                float height = Screen.height;
                float num = Screen.height * (MainBackgroundSize.x / MainBackgroundSize.y);
                position = new Rect(Screen.width / 2 - num / 2f, 0f, num, height);
            }
            else
            {
                float width = Screen.width;
                float num = Screen.width * (MainBackgroundSize.y / MainBackgroundSize.x);
                position = new Rect(0f, Screen.height / 2 - num / 2f, width, num);
            }

            GUI.DrawTexture(position, MainBackground, ScaleMode.ScaleToFit);
        }
    }
}
