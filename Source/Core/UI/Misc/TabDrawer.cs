using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public static class TabDrawer
    {
        public const float MaxTabWidth = 200f;
        public const float TabHeight = 32f;
        public const float TabHoriztonalOverlap = 10f;

        public static List<TabRecord> tabList = new List<TabRecord>();

        public static TabRecord DrawTabs(Rect baseRect, IEnumerable<TabRecord> tabsEnum)
        {
            tabList = new List<TabRecord>(tabsEnum);

            TabRecord tabClicked = null;
            var tabSelected = tabList.FirstOrDefault(tab => tab.selected);

            // no tabs selected error
            if (tabSelected == null)
            {
                return tabList[0];
            }

            var num = baseRect.width + (tabList.Count - 1) * TabHoriztonalOverlap;
            var tabWidth = num / tabList.Count;
            if (tabWidth > MaxTabWidth)
            {
                tabWidth = MaxTabWidth;
            }
            var rectTabBody = new Rect(baseRect);
            rectTabBody.y -= TabHeight;
            rectTabBody.height = 9999f;

            GUI.BeginGroup(rectTabBody);
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;

                Func<TabRecord, Rect> DefineTabHeaderDrawRect = delegate(TabRecord tab)
                {
                    var tabIndex = tabList.IndexOf(tab);
                    var xPosition = tabIndex * (tabWidth - TabHoriztonalOverlap);
                    return new Rect(xPosition, 1f, tabWidth, TabHeight);
                };

                foreach (var currentTab in tabList)
                {
                    var rectCurrentTabHeader = DefineTabHeaderDrawRect(currentTab);
                    // mouseover tab
                    if (Mouse.IsOver(rectCurrentTabHeader))
                    {
                        MouseoverSounds.DoRegion(rectCurrentTabHeader, SoundDefOf.MouseoverTab);
                    }
                    // tab clicked
                    if (Widgets.InvisibleButton(rectCurrentTabHeader))
                    {
                        // select if not selected
                        if (currentTab != tabSelected && currentTab.clickedAction != null)
                        {
                            SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
                            currentTab.clickedAction();
                            tabClicked = currentTab;
                        }
                    }
                }

                // draw all but selected tabs in reversed order (tabs overlap left to right)
                for (var tabIndex = tabList.Count - 1; tabIndex >= 0; tabIndex--)
                {
                    if (!tabList[tabIndex].selected)
                    {
                        tabList[tabIndex].Draw(DefineTabHeaderDrawRect(tabList[tabIndex]));
                    }
                }
                // draw selected tab above all others
                tabSelected.Draw(DefineTabHeaderDrawRect(tabSelected));

                Text.Anchor = TextAnchor.UpperLeft;
            }
            GUI.EndGroup();

            return tabClicked;
        }
    }
}
