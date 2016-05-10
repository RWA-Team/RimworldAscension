using UnityEngine;
using Verse;

namespace RA
{
    public static class RA_Assets
    {
        #region TEXTURES

        public static Texture2D
            Missing,
            MainMenuTitle,
            MainMenuBackground;

        #endregion

        public static void Init()
        {
            Missing = ContentFinder<Texture2D>.Get("Missing");
            MainMenuBackground = ContentFinder<Texture2D>.Get("UI/MainMenu/Background");
            MainMenuTitle = ContentFinder<Texture2D>.Get("UI/MainMenu/GameTitle");
        }
    }
}