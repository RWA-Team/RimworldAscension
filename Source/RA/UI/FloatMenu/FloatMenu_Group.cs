using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static RA.UIUtil;

namespace RA
{
    public class FloatMenu_Group : FloatMenu
    {
        public static float OptionSpacing = 10f;
        public static float MaxScreenHeightPercent = 0.8f;

        public static int MaxColumns => Mathf.FloorToInt(Screen.width/GizmoSize);
        public static float MaxWindowHeight => Screen.height*MaxScreenHeightPercent;
        public static Vector2 InitialPositionShift = new Vector2(4f, 0f);

        public float TotalHeight => Mathf.Min(MaxWindowHeight, options.Count*(GizmoSize + OptionSpacing));
        public float TotalWidth => ColumnCount*GizmoSize;
        public int ColumnCount => Mathf.Max(1, Mathf.CeilToInt(MaxWindowHeight/(GizmoSize + OptionSpacing)));

        public new List<FloatMenuOption_Group> options;

        public FloatMenu_Group(List<FloatMenuOption_Group> options)
            : base(options.Select(option => option as FloatMenuOption).ToList())
        {
            this.options = options;
        }

        public override Vector2 InitialSize => new Vector2(TotalWidth, TotalHeight);

        protected override void SetInitialSizeAndPosition()
        {
            windowRect.position = GenUI.AbsMousePosition() + InitialPositionShift;
            windowRect.y -= InitialSize.y;
            windowRect.size = InitialSize;
        }

        public override void DoWindowContents(Rect rect)
        {
            ResetText();

            var curPos = Vector2.zero;
            foreach (var option in options.OrderByDescending(opt => opt.priority))
            {
                // start new column
                if (curPos.y + GizmoSize + OptionSpacing > TotalHeight)
                {
                    curPos.y = 0f;
                    curPos.x += GizmoSize + OptionSpacing;
                }
                var optionRect = new Rect(curPos.x, curPos.y, GizmoSize, GizmoSize);
                curPos.y += GizmoSize + OptionSpacing;

                var clicked = option.DoGUI(optionRect, givesColonistOrders);
                if (clicked)
                {
                    Find.WindowStack.TryRemove(this);
                    break;
                }
            }

            if (Event.current.type == EventType.MouseDown)
            {
                Event.current.Use();
                Close();
            }
        }
    }
}