using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

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
            TabDrawer.tabList = new List<TabRecord>(tabsEnum);

            TabRecord tabClicked = null;
            TabRecord tabSelected = TabDrawer.tabList.FirstOrDefault(tab => tab.selected);

            // no tabs selected error
            if (tabSelected == null)
            {
                return TabDrawer.tabList[0];
            }

            float num = baseRect.width + (float)(TabDrawer.tabList.Count - 1) * TabHoriztonalOverlap;
            float tabWidth = num / (float)TabDrawer.tabList.Count;
            if (tabWidth > MaxTabWidth)
            {
                tabWidth = MaxTabWidth;
            }
            Rect rectTabBody = new Rect(baseRect);
            rectTabBody.y -= TabHeight;
            rectTabBody.height = 9999f;

            GUI.BeginGroup(rectTabBody);
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;

                Func<TabRecord, Rect> DefineTabHeaderDrawRect = delegate(TabRecord tab)
                {
                    int tabIndex = TabDrawer.tabList.IndexOf(tab);
                    float xPosition = (float)tabIndex * (tabWidth - TabHoriztonalOverlap);
                    return new Rect(xPosition, 1f, tabWidth, TabHeight);
                };

                foreach (TabRecord currentTab in tabList)
                {
                    Rect rectCurrentTabHeader = DefineTabHeaderDrawRect(currentTab);
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
                for (int tabIndex = tabList.Count - 1; tabIndex >= 0; tabIndex--)
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
