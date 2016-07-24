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
        public static FieldInfo info;

        public static readonly Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);
        public static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);
        public const float DragPriceDrawNumberX = 29f;
        
        public ThingDef stuffDef;
        public ThingDef thingDef;

        // assigns stuff type for minified buildables
        public RA_Designator_Build(BuildableDef entDef) : base(entDef)
        {
            thingDef = entDef as ThingDef;
            if (thingDef != null)
            {
                if (thingDef.MadeFromStuff)
                {
                    stuffDef = GenStuff.DefaultStuffFor(thingDef);
                }
                if (thingDef.Minifiable)
                {
                    stuffDef = GenStuff.DefaultStuffFor(thingDef.minifiedDef);
                }
            }
        }

        // TODO required
        // determine conditions of hiding similar gizmos
        public override bool GroupsWith(Gizmo other) => PlacingDef == (other as Designator_Build)?.PlacingDef;

        // TODO required
        // use install designator instead of build for all defs with minifiable option enabled
        public override void ProcessInput(Event ev)
        {
            var stuffDef = Initializer.GetHiddenValue(typeof(Designator_Build), this, "stuffDef", info);

            // select build options
            if (thingDef != null && (thingDef.MadeFromStuff || thingDef.Minifiable))
            {
                var buildOptions = new List<FloatMenuOption>();
                // minifiable things build options
                if (thingDef.Minifiable)
                {
                    var minifiables = Find.ListerThings.ThingsOfDef(thingDef.minifiedDef);
                    if (minifiables.Count > 0)
                    {
                        foreach (var minifiable in minifiables)
                        {
                            if (BlueprintPlaced(minifiable as MinifiedThing) ||
                                Find.Reservations.IsReserved(minifiable, Faction.OfPlayer) ||
                                minifiable.IsForbidden(Faction.OfPlayer) || minifiable.IsBurning())
                                continue;

                            var optionLabel = minifiable.TryGetComp<CompQuality>()?.CompInspectStringExtra() + minifiable.LabelCap;
                            var option = new FloatMenuOption(optionLabel, () =>
                            {
                                DesignateInstall(minifiable);
                                stuffDef = minifiable.Stuff;
                            });
                            buildOptions.Add(option);
                        }
                    }
                    else if (DebugSettings.godMode)
                    {
                        var optionLabel = thingDef.LabelCap;
                        var option = new FloatMenuOption(optionLabel, () =>
                        {
                            DesignatorManager.Select(this);
                            stuffDef = GenStuff.RandomStuffFor(thingDef.minifiedDef);
                        });
                        buildOptions.Add(option);
                    }
                }
                // vanilla build options
                else
                {
                    foreach (var resourceDef in Find.ResourceCounter.AllCountedAmounts.Keys)
                    {
                        if (resourceDef.IsStuff && resourceDef.stuffProps.CanMake(thingDef) &&
                            (DebugSettings.godMode || Find.ListerThings.ThingsOfDef(resourceDef).Count > 0))
                        {
                            var localStuffDef = resourceDef;
                            var labelCap = localStuffDef.LabelCap;
                            var item = new FloatMenuOption(labelCap, () =>
                            {
                                CurActivateSound?.PlayOneShotOnCamera();
                                DesignatorManager.Select(this);
                                stuffDef = localStuffDef;
                            });
                            buildOptions.Add(item);
                        }
                    }
                }

                if (buildOptions.Count > 0)
                {
                    // draw float menu
                    var floatMenu = new FloatMenu(buildOptions) { vanishIfMouseDistant = true };
                    Find.WindowStack.Add(floatMenu);

                    // call designator for pressing gizmo itself
                    // install designator for minified
                    if (thingDef.Minifiable)
                    {
                        buildOptions.FirstOrDefault().action();
                    }
                    // build designator for everything else
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
            //can't use build options selector if chosen def or stuff
            else
            {
                base.ProcessInput(ev);
            }
        }

        public void DesignateInstall(Thing minifiable)
        {
            // clear previous selections
            Find.Selector.ClearSelection();
            // select minifiable
            Find.Selector.Select(minifiable, true, false);
            // set local stuff the same as minifiable has
            stuffDef = minifiable.Stuff;
            // calling base method
            CurActivateSound?.PlayOneShotOnCamera();
            // select install designator for that minifiable, instead of build
            DesignatorManager.Select(new Designator_Install());
        }

        public bool BlueprintPlaced(MinifiedThing minifiedThing)
        {
            var blueprints = Find.ListerThings.ThingsMatching(ThingRequest.ForDef(minifiedThing.InnerThing.def.installBlueprintDef));

            foreach (var blueprint in blueprints)
            {
                var blueprint_Install = blueprint as Blueprint_Install;
                if (blueprint_Install?.MiniToInstallOrBuildingToReinstall == minifiedThing)
                {
                    return true;
                }
            }
            return false;
        }

        // added special case for minified things
        public override void DrawPanelReadout(ref float curY, float width)
        {
            //// special Graphic_StuffBased implementation
            //var baseGraphic = PlacingDef.graphic as Graphic_StuffBased;
            //if (stuffDef != null && baseGraphic != null)
            //    icon = (Texture2D)baseGraphic.categorizedGraphics[stuffDef.stuffProps.categories[0].defName].MatSingle.mainTexture;

            // special case for minified things
            var costList = thingDef != null && thingDef.Minifiable
                ? new List<ThingCount> { new ThingCount(thingDef, 1) }
                : PlacingDef.CostListAdjusted(stuffDef);

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
                Widgets.InfoCardButton(0f, curY, thingDef, stuffDef);
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
            if (useMouseIcon) GenUI.DrawMouseAttachment(icon, string.Empty);
            if (!RA_ArchitectCategoryTab.InfoRect.Contains(GenUI.AbsMousePosition()))
            {
                var dragger = DesignatorManager.Dragger;
                var num = dragger.Dragging ? dragger.DragCells.Count : 1;
                var num2 = 0f;
                var vector = Event.current.mousePosition + DragPriceDrawOffset;
                
                var costList = thingDef != null && thingDef.Minifiable
                    ? new List<ThingCount> {new ThingCount(thingDef, 1)}
                    : PlacingDef.CostListAdjusted(stuffDef);

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
    }
}