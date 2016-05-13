using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RA
{
    public static class UIUtil
    {
        #region CONSTANTS

        // width of scroll bar
        public const float ScrollDraggerWidth = 16f;
        // size of cross close icon
        public const float CloseIconWidth = 20f;
        // width of base margin between ITab content and Itab border
        public const float DefaultMargin = 5f;
        // height of additional ITab top indent
        public const float ITabTopIndent = 10f;
        // height of button to open ITab
        public const float ITabInvokeButtonHeight = 30f;
        // default width for ITab window
        public const float ITabWindowWidth = 432f;
        // default height for small text field
        public const float TextHeight = 25f;
        // height of main tabs selection panel
        public const float MainTabsPanelHeight = 35f;
        // height of row to draw thing icon with description
        public const float ThingRowHeight = 30f;

        #endregion

        #region COLORS

        // mouse over highlight color
        public static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f);
        public static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f);

        #endregion

        #region VANILLA_TEXTURES

        // mouse over highlight color
        public static readonly Texture2D ButtonBGTexture = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG");
        public static readonly Texture2D ButtonBGMouseoverTexture = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover");
        public static readonly Texture2D ButtonBGClickTexture = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick");

        #endregion

        public static void ResetText()
        {
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void DrawScrollableRect(Rect outerRect, Rect viewRect, ref Vector2 scrollPosition, Action drawContents)
        {
            Widgets.BeginScrollView(outerRect, ref scrollPosition, viewRect);
            drawContents();
            Widgets.EndScrollView();
        }

        public static void DrawItemsList(Rect rect, ref Vector2 scrollPosition, List<Thing> itemsList, Action<Thing> removeAction)
        {
            ResetText();
            Widgets.DrawWindowBackground(rect);
            // view rect is rect without scroll bar width
            var viewRect = new Rect(rect)
            {
                width = rect.width - ScrollDraggerWidth
            };
            DrawScrollableRect(rect, viewRect, ref scrollPosition, () =>
            {
                var curY = viewRect.y;
                if (itemsList.Any())
                {
                    foreach (var thing in itemsList)
                    {
                        var rowRect = new Rect(viewRect.x, curY, viewRect.width, ThingRowHeight).ContractedBy(1f);
                        if (Mouse.IsOver(rowRect))
                        {
                            GUI.color = HighlightColor;
                            GUI.DrawTexture(rowRect, TexUI.HighlightTex);
                        }
                        if (Widgets.InvisibleButton(rowRect) && Event.current.button == 1)
                        {
                            var list = new List<FloatMenuOption>();
                            list.Add(new FloatMenuOption("ThingInfo".Translate(), () =>
                            {
                                Find.WindowStack.Add(new Dialog_InfoCard(thing));
                            }));
                            list.Add(new FloatMenuOption("Remove", () => removeAction(thing)));
                            Find.WindowStack.Add(new FloatMenu(list, thing.LabelCap));
                        }

                        var thingIconRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height);
                        Widgets.ThingIcon(thingIconRect, thing);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        GUI.color = ThingLabelColor;
                        var thingLabelRect = new Rect(thingIconRect.xMax + DefaultMargin, thingIconRect.y,
                            rowRect.width - (thingIconRect.width + DefaultMargin), rowRect.height);
                        Widgets.Label(thingLabelRect, thing.LabelCap);

                        curY += rowRect.height;
                    }
                }
            });
            ResetText();
        }
    }
}