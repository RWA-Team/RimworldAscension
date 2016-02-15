using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class Window_ThingFilter : Window
    {
        Building_WorkTable_Fueled burner;

        public Vector2 scrollPosition = default(Vector2);

        public Window_ThingFilter(Building_WorkTable_Fueled burner, float offsetY)
        {
            this.closeOnEscapeKey = true;
            this.closeOnClickedOutside = true;

            this.currentWindowRect.width = 280f;
            this.currentWindowRect.height = 200f;
            this.currentWindowRect.x = 0f;
            this.currentWindowRect.y = offsetY + this.currentWindowRect.height;

            this.burner = burner;
        }

        public override void DoWindowContents(Rect inRect)
        {
            ThingFilterUI.DoThingFilterConfigWindow(inRect, ref this.scrollPosition, burner.filterFuelCurrent, burner.filterFuelPossible);
        }
    }

    // fixing original ThingFilterUI
    public static class ThingFilterUI
    {
        public const float ExtraViewHeight = 90f;
        public const float RangeLabelTab = 10f;
        public const float RangeLabelHeight = 19f;
        public const float SliderHeight = 26f;
        public const float SliderTab = 20f;
        public static float viewHeight;

        public static void DoThingFilterConfigWindow(Rect rect, ref Vector2 scrollPosition, ThingFilter filter, ThingFilter parentFilter = null, int openMask = 1)
        {
            Widgets.DrawMenuSection(rect, true);
            Text.Font = GameFont.Tiny;
            float num = rect.width - 2f;
            Rect rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);
            if (Widgets.TextButton(rect2, "ClearAll".Translate(), true, false))
            {
                filter.SetDisallowAll();
            }
            Rect rect3 = new Rect(rect2.xMax + 1f, rect2.y, num / 2f - 1f, 24f);
            if (Widgets.TextButton(rect3, "AllowAll".Translate(), true, false))
            {
                filter.SetAllowAll(parentFilter);
            }
            Text.Font = GameFont.Small;
            rect.yMin = rect2.yMax;
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, ThingFilterUI.viewHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float num2 = 0f;
            num2 += 2f;
            DrawHitPointsFilterConfig(ref num2, viewRect.width, filter);
            DrawQualityFilterConfig(ref num2, viewRect.width, filter);
            float num3 = num2;
            Rect rect4 = new Rect(0f, num2, 9999f, 9999f);
            Listing_TreeThingFilter listing_TreeThingFilter = new Listing_TreeThingFilter(rect4, filter, parentFilter, 210f, true);
            TreeNode_ThingCategory node = ThingCategoryNodeDatabase.RootNode;
            if (parentFilter != null)
            {
                if (parentFilter.DisplayRootCategory == null)
                {
                    parentFilter.RecalculateDisplayRootCategory();
                }
                node = parentFilter.DisplayRootCategory;
            }
            listing_TreeThingFilter.DoCategoryChildren(node, 0, openMask, true);
            listing_TreeThingFilter.End();
            if (Event.current.type == EventType.Layout)
            {
                viewHeight = num3 + listing_TreeThingFilter.CurHeight + 90f;
            }
            Widgets.EndScrollView();
        }

        public static void DrawHitPointsFilterConfig(ref float y, float width, ThingFilter filter)
        {
            if (!filter.allowedHitPointsConfigurable)
            {
                return;
            }
            Rect rect = new Rect(20f, y, width - 20f, 26f);
            FloatRange allowedHitPointsPercents = filter.AllowedHitPointsPercents;
            Widgets.FloatRange(rect, 1, ref allowedHitPointsPercents, 0f, 1f, ToStringStyle.PercentZero, "HitPoints");
            filter.AllowedHitPointsPercents = allowedHitPointsPercents;
            y += 26f;
            y += 5f;
            Text.Font = GameFont.Small;
        }

        public static void DrawQualityFilterConfig(ref float y, float width, ThingFilter filter)
        {
            if (!filter.allowedQualitiesConfigurable)
            {
                return;
            }
            Rect rect = new Rect(20f, y, width - 20f, 26f);
            QualityRange allowedQualityLevels = filter.AllowedQualityLevels;
            Widgets.QualityRange(rect, 2, ref allowedQualityLevels);
            filter.AllowedQualityLevels = allowedQualityLevels;
            y += 26f;
            y += 5f;
            Text.Font = GameFont.Small;
        }
    }
}