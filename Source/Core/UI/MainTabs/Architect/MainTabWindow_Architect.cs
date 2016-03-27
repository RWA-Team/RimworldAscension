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
        public const float WinWidth = 240f; // 200f
        public const float BuildButtonHeight = 30f;
        public const float DesignationButtonHeight = 50f;
        public const float TextHeight = 25f;

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
                return Mathf.CeilToInt(desPanelsCached.Count/2f)*BuildButtonHeight + DesignationButtonHeight +
                       TextHeight*2;
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

        public override void DoWindowContents(Rect innerRect)
        {
            base.DoWindowContents(innerRect);

            var columnWidth = innerRect.width/2f;

            // burning fillable bar
            Text.Font = GameFont.Small;
            var designationsLabelRect = new Rect(0, 0f, innerRect.width, TextHeight);
            Widgets.Label(designationsLabelRect, "Designations:");

            // orders + zones designations
            Text.Font = GameFont.Medium;
            var designationsRect = new Rect(0f, designationsLabelRect.yMax, innerRect.width, DesignationButtonHeight);
            GUI.BeginGroup(designationsRect);
            {
                for (int i = 0; i < 2; i++)
                {
                    var tab = desPanelsCached[i];
                    var columnIndex = i % 2;

                    var buttonRect = new Rect(columnIndex*columnWidth, 0, columnWidth, DesignationButtonHeight);

                    // draw buttons
                    if (WidgetsSubtle.ButtonSubtle(buttonRect, tab.def.LabelCap, 0f, 8f,
                        SoundDefOf.MouseoverButtonCategory))
                    {
                        ClickedCategory(tab);
                    }
                }
            }
            GUI.EndGroup();

            // build designations
            Text.Font = GameFont.Small;
            var buttonsRect = new Rect(0f, designationsRect.yMax, innerRect.width, innerRect.height - designationsLabelRect.yMax);
            GUI.BeginGroup(buttonsRect);
            {
                for (int i = 2; i < desPanelsCached.Count; i++)
                {
                    var tab = desPanelsCached[i];
                    var columnIndex = i % 2;
                    var rowIndex = i / 2;

                    var buttonRect = new Rect(columnIndex * columnWidth, rowIndex * BuildButtonHeight, columnWidth,
                        BuildButtonHeight);

                    // draw buttons
                    if (WidgetsSubtle.ButtonSubtle(buttonRect, tab.def.LabelCap, 0f, 8f,
                        SoundDefOf.MouseoverButtonCategory))
                    {
                        ClickedCategory(tab);
                    }
                }
            }
            GUI.EndGroup();
        }

        // determines the DesignationCategories to show in the menu list
        public void CacheDesPanels()
        {
            desPanelsCached = new List<ArchitectCategoryTab>();
            foreach (
                var category in
                    DefDatabase<DesignationCategoryDef>.AllDefs.Where(cat => cat.resolvedDesignators.Count > 2)
                        .OrderByDescending(des => des.order))
            {
                desPanelsCached.Add(new ArchitectCategoryTab(category));
            }
        }

        public void ClickedCategory(ArchitectCategoryTab Pan)
        {
            selectedDesPanel = selectedDesPanel == Pan ? null : Pan;
            SoundDefOf.ArchitectCategorySelect.PlayOneShotOnCamera();
        }
    }
}
