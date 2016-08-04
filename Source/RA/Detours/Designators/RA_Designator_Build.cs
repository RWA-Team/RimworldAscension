using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public class RA_Designator_Build : Designator_Build
    {
        public static FieldInfo infoStuff;

        public static readonly Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);
        public static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);
        public const float DragPriceDrawNumberX = 29f;

        // inheritance requirement
        public RA_Designator_Build(BuildableDef entDef) : base(entDef)
        {
        }

        // constructor for this designator, of subdesignator is selected
        //public RA_Designator_Build(Designator_Place designator) : base(designator.PlacingDef) { }

        // gets/sets stuff to the inner hidden stuff def
        public ThingDef Stuff
        {
            get { return Initializer.GetHiddenValue(typeof (Designator_Build), this, "stuffDef", infoStuff) as ThingDef; }
            set { Initializer.SetHiddenValue(value, typeof (Designator_Build), this, "stuffDef", infoStuff); }
        }

        // TODO
        // determine conditions of hiding similar gizmos
        public override bool GroupsWith(Gizmo other) => PlacingDef == (other as Designator_Build)?.PlacingDef;

        // use install designator instead of build for all defs with minifiable option enabled
        public override void ProcessInput(Event ev)
        {
            // play click sound
            CurActivateSound?.PlayOneShotOnCamera();

            var thingDef = PlacingDef as ThingDef;
            // select build options
            if (thingDef != null && (thingDef.MadeFromStuff || thingDef.Minifiable))
            {
                var buildOptions = GenerateBuildOptions(thingDef);

                if (buildOptions.Count > 0)
                {
                    // draw float menu
                    var floatMenu = new FloatMenu(buildOptions) {vanishIfMouseDistant = true};
                    Find.WindowStack.Add(floatMenu);

                    if (thingDef.Minifiable)
                    {
                        // selects first option from list on pressing gizmo itself
                        buildOptions.FirstOrDefault().action();
                    }
                    // current build designator for everything else
                    else
                    {
                        DesignatorManager.Select(this);
                    }
                }
                // no build options
                else
                {
                    Messages.Message("NoStuffsToBuildWith".Translate(), MessageSound.RejectInput);
                }
            }
            // select designator as it is
            else
            {
                DesignatorManager.Select(this);
            }
        }

        public List<FloatMenuOption> GenerateBuildOptions(ThingDef thingDef)
        {
            var buildOptions = new List<FloatMenuOption>();

            // minifiables build options
            if (thingDef.Minifiable)
            {
                // god mode build options
                if (DebugSettings.godMode)
                {
                    var optionLabel = thingDef.LabelCap;
                    var option = new FloatMenuOption(optionLabel, () =>
                    {
                        DesignatorManager.Select(this);
                        Stuff = GenStuff.DefaultStuffFor(thingDef);
                    });
                    buildOptions.Add(option);
                }
                // search for all available minified things with proper parent def on map
                else
                {
                    var minifiables = Find.ListerThings.ThingsOfDef(thingDef.minifiedDef);
                    if (minifiables.Count > 0)
                    {
                        foreach (var minifiable in minifiables)
                        {
                            if (AlreadyInstalled(minifiable as MinifiedThing) ||
                                Find.Reservations.IsReserved(minifiable, Faction.OfPlayer) ||
                                minifiable.IsForbidden(Faction.OfPlayer) || minifiable.IsBurning())
                                continue;

                            var optionLabel = minifiable.TryGetComp<CompQuality>()?.CompInspectStringExtra() +
                                              minifiable.LabelCap;
                            var option = new FloatMenuOption(optionLabel, () =>
                            {
                                DesignateInstall(minifiable);
                                Stuff = minifiable.Stuff;
                            });
                            buildOptions.Add(option);
                        }
                    }
                }
            }
            // stuff based build options
            else
            {
                foreach (var resourceDef in Find.ResourceCounter.AllCountedAmounts.Keys)
                {
                    if (resourceDef.IsStuff && resourceDef.stuffProps.CanMake(thingDef) &&
                        (DebugSettings.godMode || Find.ListerThings.ThingsOfDef(resourceDef).Count > 0))
                    {
                        var labelCap = resourceDef.LabelCap;
                        var item = new FloatMenuOption(labelCap, () =>
                        {
                            DesignatorManager.Select(this);
                            Stuff = resourceDef;
                        });
                        buildOptions.Add(item);
                    }
                }
            }

            return buildOptions;
        }

        // added special case for minified things
        public override void DrawPanelReadout(ref float curY, float width)
        {
            var thingDef = PlacingDef as ThingDef;

            // special case for minified things
            var costList = thingDef != null && thingDef.Minifiable
                ? new List<ThingCount> {new ThingCount(thingDef, 1)}
                : PlacingDef.CostListAdjusted(Stuff);

            UIUtil.ResetText();
            foreach (var thingCount in costList)
            {
                GUI.DrawTexture(new Rect(0f, curY, 20f, 20f), thingCount.thingDef.uiIcon);
                if (thingCount.thingDef != null &&
                    thingCount.thingDef.resourceReadoutPriority != ResourceCountPriority.Uncounted &&
                    Find.ResourceCounter.GetCount(thingCount.thingDef) < thingCount.count)
                {
                    GUI.color = Color.red;
                }
                Widgets.Label(new Rect(26f, curY + 2f, 50f, 100f), thingCount.count.ToString());
                GUI.color = Color.white;

                // special case for minified things
                var text = thingCount.thingDef == null
                    ? "(" + "UnchosenStuff".Translate() + ")"
                    : thingCount.thingDef.Minifiable
                        ? "Minified " + thingCount.thingDef.LabelCap
                        : thingCount.thingDef.LabelCap;

                var width2 = width - 60f;
                var num = Text.CalcHeight(text, width2) - 2f;
                Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
                curY += num;
            }

            if (thingDef != null)
            {
                Widgets.InfoCardButton(0f, curY, thingDef, Stuff);
            }
            else
            {
                Widgets.InfoCardButton(0f, curY, PlacingDef);
            }
            curY += 24f;
        }

        // draws texture and build cost while building
        // modified call for InfoRect
        // added special case for minified things
        public override void DrawMouseAttachments()
        {
            var thingDef = PlacingDef as ThingDef;
            if (useMouseIcon) GenUI.DrawMouseAttachment(icon, string.Empty);
            if (!ArchitectCategoryTab.InfoRect.Contains(GenUI.AbsMousePosition()))
            {
                var dragger = DesignatorManager.Dragger;
                var num = dragger.Dragging ? dragger.DragCells.Count : 1;
                var num2 = 0f;
                var vector = Event.current.mousePosition + DragPriceDrawOffset;

                var costList = thingDef != null && thingDef.Minifiable
                    ? new List<ThingCount> {new ThingCount(thingDef, 1)}
                    : PlacingDef.CostListAdjusted(Stuff);

                UIUtil.ResetText();
                foreach (var thingCount in costList)
                {
                    var top = vector.y + num2;
                    var position = new Rect(vector.x, top, 27f, 27f);
                    GUI.DrawTexture(position, thingCount.thingDef.uiIcon);
                    var rect = new Rect(vector.x + 29f, top, 999f, 29f);
                    var num3 = num*thingCount.count;
                    var text = num3.ToString();
                    if (Find.ResourceCounter.GetCount(thingCount.thingDef) < num3)
                    {
                        GUI.color = Color.red;
                        text = text + " (" + "NotEnoughStoredLower".Translate() + ")";
                    }
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(rect, text);
                    UIUtil.ResetText();
                    num2 += 29f;
                }
            }
        }

        public void DesignateInstall(Thing minifiable)
        {
            // clear previous selections
            Find.Selector.ClearSelection();
            // select minifiable
            Find.Selector.Select(minifiable, true, false);
            // select install designator for that minifiable, instead of build
            DesignatorManager.Select(new Designator_Install());
        }

        public bool AlreadyInstalled(MinifiedThing minifiedThing)
        {
            return
                Find.ListerThings.ThingsMatching(ThingRequest.ForDef(minifiedThing.InnerThing.def.installBlueprintDef))
                    .Select(blueprint => blueprint as Blueprint_Install)
                    .Any(blueprint_Install => blueprint_Install?.MiniToInstallOrBuildingToReinstall == minifiedThing);
        }

        // make vanilla build gizmo open building groups
        public override GizmoResult GizmoOnGUI(Vector2 topLeft)
        {
            // start GUI.color is the transparency set by our floatmenu parent
            // store it, so we can apply it to all subsequent colours
            var transparency = GUI.color;
            
            var buttonRect = new Rect(topLeft.x, topLeft.y, Width, 75f);
            var mouseover = false;
            if (Mouse.IsOver(buttonRect))
            {
                mouseover = true;
                GUI.color = GenUI.MouseoverColor*transparency;
            }
            var tex = icon ?? BaseContent.BadTex;
            GUI.DrawTexture(buttonRect, BGTex);
            MouseoverSounds.DoRegion(buttonRect, SoundDefOf.MouseoverCommand);
            GUI.color = IconDrawColor*transparency;
            Widgets.DrawTextureFitted(new Rect(buttonRect), tex, iconDrawScale*0.85f, iconProportions, iconTexCoords);
            GUI.color = Color.white*transparency;
            var clicked = false;
            var keyCode = hotKey?.MainKey ?? KeyCode.None;
            if (keyCode != KeyCode.None && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
            {
                Widgets.Label(new Rect(buttonRect.x + 5f, buttonRect.y + 5f, 16f, 18f), keyCode.ToString());
                GizmoGridDrawer.drawnHotKeys.Add(keyCode);
                if (hotKey.KeyDownEvent)
                {
                    clicked = true;
                    Event.current.Use();
                }
            }
            if (Widgets.ButtonInvisible(buttonRect))
                clicked = true;
            var labelCap = LabelCap;
            if (!labelCap.NullOrEmpty())
            {
                var height = Text.CalcHeight(labelCap, buttonRect.width) - 2f;
                var rect2 = new Rect(buttonRect.x, (float) (buttonRect.yMax - (double) height + 12.0), buttonRect.width,
                    height);
                GUI.DrawTexture(rect2, TexUI.GrayTextBG);
                GUI.color = Color.white*transparency;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect2, labelCap);
                Text.Anchor = TextAnchor.UpperLeft;
            }
            GUI.color = Color.white;
            if (DoTooltip)
            {
                TipSignal tip = Desc;
                if (disabled && !disabledReason.NullOrEmpty())
                {
                    var local = @tip;
                    local.text += "\n\nDISABLED: " + disabledReason;
                }
                TooltipHandler.TipRegion(buttonRect, tip);
            }
            if (!tutorHighlightTag.NullOrEmpty())
                TutorUIHighlighter.HighlightOpportunity(tutorHighlightTag, buttonRect);


            if (clicked)
            {
                if (!disabled)
                    return new GizmoResult(GizmoState.Interacted, Event.current);
                if (!disabledReason.NullOrEmpty())
                    Messages.Message(disabledReason, MessageSound.RejectInput);
                return new GizmoResult(GizmoState.Mouseover, null);
            }
            return mouseover ? new GizmoResult(GizmoState.Mouseover, null) : new GizmoResult(GizmoState.Clear, null);
        }
    }
}