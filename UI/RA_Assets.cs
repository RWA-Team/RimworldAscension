using UnityEngine;
using Verse;

namespace RA
{
    public static class RA_Assets
    {
        #region TEXTURES

        public static Texture2D
            FullTexFuel,
            FullTexFuelCount,
            FullTexBurnerLow,
            FullTexBurnerHight,
            EmptyTex,
            IconBGTex,
            Missing,
            MainMenuTitle,
            MainMenuBackground;

        #endregion

        public static void Init()
        {
            Missing = ContentFinder<Texture2D>.Get("Missing");
            MainMenuBackground = ContentFinder<Texture2D>.Get("UI/MainMenu/Background");
            MainMenuTitle = ContentFinder<Texture2D>.Get("UI/MainMenu/GameTitle");
            IconBGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

            FullTexFuel = SolidColorMaterials.NewSolidColorTexture(new Color(0.7f, 0.7f, 1f));
            FullTexFuelCount = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0.7f, 1f));
            FullTexBurnerLow = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.41f, 0f));
            FullTexBurnerHight = SolidColorMaterials.NewSolidColorTexture(new Color(0.75f, 0f, 0f));
            EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.gray);
        }
    }
}