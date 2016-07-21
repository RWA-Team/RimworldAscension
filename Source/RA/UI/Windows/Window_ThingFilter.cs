using UnityEngine;
using Verse;

namespace RA
{
    public class Window_ThingFilter : Window
    {
        CompFueled burner;

        public Vector2 scrollPosition = default(Vector2);

        public Window_ThingFilter(CompFueled burner, float offsetY)
        {
            closeOnEscapeKey = true;
            closeOnClickedOutside = true;

            windowRect.width = 280f;
            windowRect.height = 200f;
            windowRect.x = 0f;
            windowRect.y = offsetY + windowRect.height;

            this.burner = burner;
        }

        public override void DoWindowContents(Rect inRect)
        {
            ThingFilterUI.DoThingFilterConfigWindow(inRect, ref scrollPosition, burner.filterFuelCurrent, burner.filterFuelPossible);
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
            Widgets.DrawMenuSection(rect);
            Text.Font = GameFont.Tiny;
            var num = rect.width - 2f;
            var rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);
            if (Widgets.ButtonText(rect2, "ClearAll".Translate()))
            {
                filter.SetDisallowAll();
            }
            var rect3 = new Rect(rect2.xMax + 1f, rect2.y, num / 2f - 1f, 24f);
            if (Widgets.ButtonText(rect3, "AllowAll".Translate()))
            {
                filter.SetAllowAll(parentFilter);
            }
            Text.Font = GameFont.Small;
            rect.yMin = rect2.yMax;
            var viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            var num2 = 0f;
            num2 += 2f;
            DrawHitPointsFilterConfig(ref num2, viewRect.width, filter);
            DrawQualityFilterConfig(ref num2, viewRect.width, filter);
            var num3 = num2;
            var rect4 = new Rect(0f, num2, 9999f, 9999f);
            var listing_TreeThingFilter = new Listing_TreeThingFilter(rect4, filter, parentFilter);
            var node = ThingCategoryNodeDatabase.RootNode;
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
            var rect = new Rect(20f, y, width - 20f, 26f);
            var allowedHitPointsPercents = filter.AllowedHitPointsPercents;
            Widgets.FloatRange(rect, 1, ref allowedHitPointsPercents, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
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
            var rect = new Rect(20f, y, width - 20f, 26f);
            var allowedQualityLevels = filter.AllowedQualityLevels;
            Widgets.QualityRange(rect, 2, ref allowedQualityLevels);
            filter.AllowedQualityLevels = allowedQualityLevels;
            y += 26f;
            y += 5f;
            Text.Font = GameFont.Small;
        }
    }
}