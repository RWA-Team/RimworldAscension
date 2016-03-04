using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public class MainTabWindow_Architect : MainTabWindow
    {
        public const float WinWidth = 200f;

        public const float ButHeight = 32f;

        public List<ArchitectCategoryTab> desPanelsCached;

        public ArchitectCategoryTab selectedDesPanel;

        public float WinHeight
        {
            get
            {
                if (desPanelsCached == null)
                {
                    CacheDesPanels();
                }
                return Mathf.CeilToInt(desPanelsCached.Count / 2f) * 32f;
            }
        }

        public override Vector2 RequestedTabSize => new Vector2(200f, WinHeight);

        protected override float WindowPadding => 0f;

        public MainTabWindow_Architect()
        {

            CacheDesPanels();
        }

        public override void ExtraOnGUI()
        {
            base.ExtraOnGUI();
            selectedDesPanel?.DesignationTabOnGUI();
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Text.Font = GameFont.Small;
            var num = inRect.width / 2f;
            var num2 = 0f;
            var num3 = 0f;
            foreach (ArchitectCategoryTab tab in desPanelsCached)
            {
                var rect = new Rect(num2 * num, num3 * 32f, num, 32f);
                rect.height += 1f;
                if (num2 == 0f)
                {
                    rect.width += 1f;
                }
                if (WidgetsSubtle.ButtonSubtle(rect, tab.def.LabelCap, 0f, 8f, SoundDefOf.MouseoverButtonCategory))
                {
                    ClickedCategory(tab);
                }
                num2 += 1f;
                if (num2 > 1f)
                {
                    num2 = 0f;
                    num3 += 1f;
                }
            }
        }

        public void CacheDesPanels()
        {
            desPanelsCached = new List<ArchitectCategoryTab>();
            foreach (var current in from dc in DefDatabase<DesignationCategoryDef>.AllDefs
                                                       orderby dc.order descending
                                                       select dc)
            {
                desPanelsCached.Add(new ArchitectCategoryTab(current));
            }
        }

        public void ClickedCategory(ArchitectCategoryTab Pan)
        {
            selectedDesPanel = selectedDesPanel == Pan ? null : Pan;
            SoundDefOf.ArchitectCategorySelect.PlayOneShotOnCamera();
        }
    }
}
