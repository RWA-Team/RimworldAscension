using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RA
{
    public class FloatMenu_Group : FloatMenu
    {
        private const float OptionSpacing = -1f;
        private const float MaxScreenHeightPercent = 0.9f;

        public static float MaxWindowHeight => Screen.height * MaxScreenHeightPercent;
        public static Vector2 InitialOffset = new Vector2(4f, 0f);
        public static float OptionSize => UIUtil.GizmoSize;
        public new static float Margin => UIUtil.DefaultMargin;

        public new List<FloatMenuOption_Group> options;

        // copypasta from base (privates, ugh)
        public Color baseColor;
        //public int columns = 1;

        public FloatMenu_Group(List<FloatMenuOption_Group> options)
            : base(options.Select(option => option as FloatMenuOption).ToList())
        {
            this.options = options;
        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();

            // move window position so it goes up from the mouse click instead of down
            windowRect.y -= windowRect.height;
        }

        public override void DoWindowContents(Rect canvas)
        {
            // fall back on base implementation if options are not configurable options.
            if (options == null)
                base.DoWindowContents(canvas);

            // define our own implementation, mostly copy-pasta with a few edits for option sizes
            // actual drawing is handled in DoGUI.
            UpdateBaseColor();
            GUI.color = baseColor;
            Text.Font = GameFont.Small;
            var row = 0;
            var column = 0;

            if (options.NullOrEmpty())
                return;

            Text.Font = GameFont.Tiny;
            foreach (var option in options.OrderByDescending(op => op.priority))
            {
                var optionRect = new Rect(InitialOffset.x + column*(OptionSize + Margin),
                    InitialOffset.y + row*(OptionSize + Margin),
                    OptionSize, OptionSize);

                // re-set transparent base color for each item.
                Widgets.DrawBox(optionRect);

                GUI.color = baseColor;
                var clicked = option.DoGUI(optionRect, givesColonistOrders);
                if (clicked)
                {
                    Find.WindowStack.TryRemove(this);
                    return;
                }
                row++;
                if (row >= columns)
                {
                    row = 0;
                    column++;
                }
            }
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }

        public float TotalWindowHeight => Mathf.Min(TotalViewHeight, MaxWindowHeight) + 1f;

        public float TotalViewHeight
        {
            get
            {
                var num = 0f;
                var num2 = 0f;
                var maxViewHeight = MaxViewHeight;
                foreach (var requiredHeight in options.Select(option => option.RequiredHeight))
                {
                    if (num2 + requiredHeight + OptionSpacing > maxViewHeight)
                    {
                        if (num2 > num)
                        {
                            num = num2;
                        }
                        num2 = requiredHeight;
                    }
                    else
                    {
                        num2 += requiredHeight + OptionSpacing;
                    }
                }
                return Mathf.Max(num, num2);
            }
        }

        public float MaxViewHeight
        {
            get
            {
                if (UsingScrollbar)
                {
                    var num = 0f;
                    var num2 = 0f;
                    foreach (var requiredHeight in options.Select(option => option.RequiredHeight))
                    {
                        if (requiredHeight > num)
                        {
                            num = requiredHeight;
                        }
                        num2 += requiredHeight + OptionSpacing;
                    }
                    var columnCount = ColumnCount;
                    num2 += columnCount * num;
                    return num2 / columnCount;
                }
                return MaxWindowHeight;
            }
        }

        // TODO: add margin?
        public float TotalWidth => UsingScrollbar
            ? ColumnCount* ColumnWidth + UIUtil.ScrollDraggerWidth
            : ColumnCount*ColumnWidth;

        public bool UsingScrollbar => ColumnCountIfNoScrollbar > MaxColumns;

        public int ColumnCount => Mathf.Min(ColumnCountIfNoScrollbar, MaxColumns);

        public int MaxColumns => Mathf.FloorToInt((Screen.width - 16f) / ColumnWidth);

        // +
        public float ColumnWidth => OptionSize;

        public int ColumnCountIfNoScrollbar
        {
            get
            {
                if (options.NullOrEmpty())
                {
                    return 1;
                }

                var columns = 1;
                var currentHeight = 0f;
                foreach (var requiredHeight in options.Select(option => option.RequiredHeight))
                {
                    if (currentHeight + requiredHeight + OptionSpacing > MaxWindowHeight)
                    {
                        columns++;
                        currentHeight = requiredHeight;
                    }
                    else
                    {
                        currentHeight += requiredHeight + OptionSpacing;
                    }
                }
                return columns;
            }
        }



        //public float MaxOptionsPerColumn => options.Count%columns == 0
        //    ? options.Count/columns
        //    : options.Count/columns + 1;

        public void UpdateBaseColor()
        {
            baseColor = Color.white;
            if (!vanishIfMouseDistant)
                return;
            var r = windowRect.ContractedBy(-12f);
            if (r.Contains(Event.current.mousePosition))
                return;
            var distanceFromRect = GenUI.DistFromRect(r, Event.current.mousePosition);
            baseColor = new Color(1f, 1f, 1f, (float) (1.0 - distanceFromRect/200.0));
            if (distanceFromRect <= 200.0)
                return;
            Close(false);
            Cancel();
        }
    }
}