using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public class RA_MainTabWindow_Architect : MainTabWindow
    {
        public const float WinWidth = 200f;
        public const float BuildButtonHeight = 30f;
        public const float DesignationButtonHeight = 40f;

        public List<RA_ArchitectCategoryTab> categories;
        public RA_ArchitectCategoryTab selectedTab;

        public float WinHeight
            => Mathf.CeilToInt((float) (categories.Count - 2)/2)*BuildButtonHeight + DesignationButtonHeight +
               UIUtil.TextHeight*(categories.Count > 2 ? 2 : 1);

        public override void PreOpen()
        {
            base.PreOpen();
            CacheTabs();
            windowRect = new Rect(0, UIUtil.MainTabsPanelHeight + WinHeight, WinWidth, WinHeight);
        }

        // determines the DesignationCategories to show in the menu list
        public void CacheTabs()
        {
            categories = new List<RA_ArchitectCategoryTab>();
            foreach (var category in DefDatabase<DesignationCategoryDef>.AllDefs
                .Where(cat => cat.ResolvedAllowedDesignators.Any(designator => designator.Visible))
                        .OrderByDescending(des => des.order))
            {
                categories.Add(new RA_ArchitectCategoryTab(category));
            }
        }

        public override void DoWindowContents(Rect innerRect)
        {
            base.DoWindowContents(innerRect);

            var columnWidth = innerRect.width/2f;

            // burning fillable bar
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            var designationsLabelRect = new Rect(0, 0f, innerRect.width, UIUtil.TextHeight);
            Widgets.Label(designationsLabelRect, "Designations:");

            // orders + zones designations
            var designationsRect = new Rect(0f, designationsLabelRect.yMax, innerRect.width, DesignationButtonHeight);
            GUI.BeginGroup(designationsRect);
            {
                for (var i = 0; i < 2; i++)
                {
                    var columnIndex = i%2;

                    // draw buttons
                    var buttonRect = new Rect(columnIndex*columnWidth, 0, columnWidth, DesignationButtonHeight);
                    if (Widgets.ButtonTextSubtle(buttonRect, categories[i].def.LabelCap, 0f, 8f,
                        SoundDefOf.MouseoverCategory))
                    {
                        ClickedCategory(categories[i]);
                    }
                }
            }
            GUI.EndGroup();

            if (categories.Count > 2)
            {
                Text.Anchor = TextAnchor.MiddleCenter;

                var buildingLabelRect = new Rect(0, designationsRect.yMax, innerRect.width, UIUtil.TextHeight);
                Widgets.Label(buildingLabelRect, "Building Categories:");

                // build designations
                var buttonsRect = new Rect(0f, buildingLabelRect.yMax, innerRect.width,
                    innerRect.height - designationsLabelRect.yMax);
                GUI.BeginGroup(buttonsRect);
                {
                    for (var i = 2; i < categories.Count; i++)
                    {
                        var columnIndex = i%2;
                        var rowIndex = i/2 - 1;

                        var buttonRect = new Rect(columnIndex*columnWidth, rowIndex*BuildButtonHeight, columnWidth,
                            BuildButtonHeight);

                        // draw buttons
                        if (Widgets.ButtonTextSubtle(buttonRect, categories[i].def.LabelCap, 0f, 8f,
                            SoundDefOf.MouseoverCategory))
                        {
                            ClickedCategory(categories[i]);
                        }
                    }
                }
                GUI.EndGroup();
            }
        }

        public void ClickedCategory(RA_ArchitectCategoryTab tab)
        {
            selectedTab = selectedTab != tab ? tab : null;
            SoundDefOf.ArchitectCategorySelect.PlayOneShotOnCamera();
        }

        public override void ExtraOnGUI()
        {
            selectedTab?.DesignationTabOnGUI();
        }
    }
}