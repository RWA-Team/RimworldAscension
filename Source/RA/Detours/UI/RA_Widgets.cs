using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    [StaticConstructorOnStartup]
    public class RA_Widgets
    {
        public static readonly Texture2D ButtonSubtleAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonSubtleAtlas");
        public static readonly Texture2D ButtonBarTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(78, 109, 129, 130).ToColor);

        public static bool ButtonTextSubtle(Rect rect, string label, float barPercent = 0f, float textLeftMargin = -1f, SoundDef mouseoverSound = null)
        {
            var mouseOver = false;
            if (Mouse.IsOver(rect))
            {
                mouseOver = true;
                GUI.color = GenUI.MouseoverColor;
            }
            if (mouseoverSound != null)
            {
                MouseoverSounds.DoRegion(rect, mouseoverSound);
            }
            Widgets.DrawAtlas(rect, ButtonSubtleAtlas);
            GUI.color = Color.white;
            if (barPercent > 0.001f)
            {
                var rect2 = rect.ContractedBy(1f);
                Widgets.FillableBar(rect2, barPercent, ButtonBarTex, null, false);
            }
            var innerRect = new Rect(rect);
            if (mouseOver)
            {
                innerRect.x += 2f;
                innerRect.y -= 2f;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            Text.WordWrap = false;
            Widgets.Label(innerRect, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            return Widgets.ButtonInvisible(rect);
        }
    }
}
