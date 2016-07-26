using UnityEngine;
using Verse;

namespace RA
{
    [StaticConstructorOnStartup]
    public static class CommonTextures
    {
        #region TEXTURES

        public static Texture2D
            FullTexFuel,
            FullTexFuelCount,
            FullTexBurnerLow,
            FullTexBurnerHight,
            EmptyTex,
            IconBGTex,
            Missing;

        #endregion

        static CommonTextures()
        {
            Missing = ContentFinder<Texture2D>.Get("Missing");
            IconBGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

            FullTexFuel = SolidColorMaterials.NewSolidColorTexture(new Color(0.7f, 0.7f, 1f));
            FullTexFuelCount = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0.7f, 1f));
            FullTexBurnerLow = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.41f, 0f));
            FullTexBurnerHight = SolidColorMaterials.NewSolidColorTexture(new Color(0.75f, 0f, 0f));
            EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.gray);
        }
    }
}