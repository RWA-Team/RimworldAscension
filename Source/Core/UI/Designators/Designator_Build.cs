using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class Designator_Build : Designator_Place
    {
        public const float DragPriceDrawNumberX = 29f;

        public BuildableDef entDef;

        public static readonly Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);

        public static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);

        public override BuildableDef PlacingDef => entDef;

        // determine conditions of hiding similar gizmos. Added Graphic_StuffBased support
        public override bool GroupsWith(Gizmo other)
        {
            return base.GroupsWith(other) || PlacingDef == (other as Designator_Build)?.PlacingDef;
        }

        public override string Label
        {
            get
            {
                var thingDef = entDef as ThingDef;
                return thingDef != null ? GenLabel.ThingLabel(thingDef, stuffDef) : entDef.label;
            }
        }

        public override string Desc => entDef.description;

        protected override Color IconDrawColor => stuffDef?.stuffProps.color ?? entDef.IconDrawColor;

        public override bool Visible
        {
            get
            {
                if (Game.GodMode)
                {
                    return true;
                }
                if (entDef.researchPrerequisite != null && !Find.ResearchManager.IsFinished(entDef.researchPrerequisite))
                {
                    return false;
                }
                if (entDef.buildingPrerequisites != null)
                {
                    return entDef.buildingPrerequisites.All(t => Find.ListerBuildings.ColonistsHaveBuilding(t));
                }
                return true;
            }
        }

        public override int DraggableDimensions => entDef.placingDraggableDimensions;

        public override bool DragDrawMeasurements => true;

        public Designator_Build(BuildableDef entDef)
        {
            this.entDef = entDef;
            icon = entDef.uiIcon;
            var thingDef = entDef as ThingDef;
            if (thingDef != null)
            {
                iconProportions = thingDef.graphicData.drawSize;
                iconDrawScale = GenUI.IconDrawScale(thingDef);
            }
            else
            {
                iconProportions = new Vector2(1f, 1f);
                iconDrawScale = 1f;
            }
            var terrainDef = entDef as TerrainDef;
            if (terrainDef != null)
            {
                iconTexCoords = new Rect(0f, 0f, TerrainTextureCroppedSize.x / icon.width, TerrainTextureCroppedSize.y / icon.height);
            }
            if (thingDef != null && thingDef.MadeFromStuff)
            {
                stuffDef = GenStuff.DefaultStuffFor(thingDef);
            }
        }

        public override void DrawMouseAttachments()
        {
            base.DrawMouseAttachments();
            if (!ArchitectCategoryTab.InfoRect.Contains(GenUI.AbsMousePosition()))
            {
                var dragger = DesignatorManager.Dragger;
                var num = dragger.Dragging ? dragger.DragCells.Count : 1;
                var num2 = 0f;
                var vector = Event.current.mousePosition + DragPriceDrawOffset;
                var list = entDef.CostListAdjusted(stuffDef);
                foreach (var thingCount in list)
                {
                    var top = vector.y + num2;
                    var position = new Rect(vector.x, top, 27f, 27f);
                    GUI.DrawTexture(position, thingCount.thingDef.uiIcon);
                    var rect = new Rect(vector.x + 29f, top, 999f, 29f);
                    var num3 = num * thingCount.count;
                    var text = num3.ToString();
                    if (Find.ResourceCounter.GetCount(thingCount.thingDef) < num3)
                    {
                        GUI.color = Color.red;
                        text = text + " (" + "NotEnoughStoredLower".Translate() + ")";
                    }
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(rect, text);
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.color = Color.white;
                    num2 += 29f;
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            var thingDef = entDef as ThingDef;
            if (thingDef == null || !thingDef.MadeFromStuff)
            {
                base.ProcessInput(ev);
            }
            else
            {
                var list = new List<FloatMenuOption>();
                foreach (var current in Find.ResourceCounter.AllCountedAmounts.Keys)
                {
                    if (current.IsStuff && current.stuffProps.CanMake(thingDef) &&
                        (Game.GodMode || Find.ListerThings.ThingsOfDef(current).Count > 0))
                    {
                        var localStuffDef = current;
                        var labelCap = localStuffDef.LabelCap;
                        var item = new FloatMenuOption(labelCap, delegate
                        {
                            ProcessInput(ev);
                            DesignatorManager.Select(this);
                            stuffDef = localStuffDef;
                        });
                        list.Add(item);
                    }
                }
                if (list.Count == 0)
                {
                    Messages.Message("NoStuffsToBuildWith".Translate(), MessageSound.RejectInput);
                }
                else
                {
                    var floatMenu = new FloatMenu(list) {vanishIfMouseDistant = true};
                    Find.WindowStack.Add(floatMenu);
                    DesignatorManager.Select(this);
                }
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return GenConstruct.CanPlaceBlueprintAt(entDef, c, placingRot, Game.GodMode);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (Game.GodMode || entDef.GetStatValueAbstract(StatDefOf.WorkToMake, stuffDef) == 0f)
            {
                if (entDef is TerrainDef)
                {
                    Find.TerrainGrid.SetTerrain(c, (TerrainDef)entDef);
                }
                else
                {
                    var thing = ThingMaker.MakeThing((ThingDef)entDef, stuffDef);
                    thing.SetFactionDirect(Faction.OfColony);
                    GenSpawn.Spawn(thing, c, placingRot);
                }
            }
            else
            {
                GenSpawn.WipeExistingThings(c, placingRot, entDef.blueprintDef, true);
                GenConstruct.PlaceBlueprintForBuild(entDef, c, placingRot, Faction.OfColony, stuffDef);
            }
            MoteThrower.ThrowMetaPuffs(GenAdj.OccupiedRect(c, placingRot, entDef.Size));
            if (entDef == ThingDef.Named("OrbitalTradeBeacon"))
            {
                ConceptDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);
            }
        }

        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            var intVec = Gen.MouseCell();
            var thingDef = entDef as ThingDef;
            if (thingDef != null && (thingDef.EverTransmitsPower || thingDef.ConnectToPower))
            {
                OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
                if (thingDef.ConnectToPower)
                {
                    var compPower = PowerConnectionMaker.BestTransmitterForConnector(intVec);
                    if (compPower != null)
                    {
                        PowerNetGraphics.RenderAnticipatedWirePieceConnecting(intVec, compPower.parent);
                    }
                }
            }
        }

        public override void DrawPanelReadout(ref float curY, float width)
        {
            // special Graphic_StuffBased implementation
            var baseGraphic = PlacingDef.graphic as Graphic_StuffBased;
            if (stuffDef != null && baseGraphic!=null)
                icon =  (Texture2D)baseGraphic.categorizedGraphics[stuffDef.stuffProps.categories[0].defName].MatSingle.mainTexture;

            if (entDef.costStuffCount <= 0 && stuffDef != null)
            {
                stuffDef = null;
            }
            Text.Font = GameFont.Tiny;
            var list = entDef.CostListAdjusted(stuffDef, false);
            foreach (var thingCount in list)
            {
                var image = thingCount.thingDef == null ? TexUI.UnknownThing : thingCount.thingDef.uiIcon;

                GUI.DrawTexture(new Rect(0f, curY, 20f, 20f), image);
                if (thingCount.thingDef != null && thingCount.thingDef.resourceReadoutPriority != ResourceCountPriority.Uncounted && Find.ResourceCounter.GetCount(thingCount.thingDef) < thingCount.count)
                {
                    GUI.color = Color.red;
                }
                Widgets.Label(new Rect(26f, curY + 2f, 50f, 100f), thingCount.count.ToString());
                GUI.color = Color.white;

                var text = thingCount.thingDef == null
                    ? "(" + "UnchosenStuff".Translate() + ")"
                    : thingCount.thingDef.LabelCap;

                var width2 = width - 60f;
                var num = Text.CalcHeight(text, width2) - 2f;
                Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
                curY += num;
            }
            var thingDef = entDef as ThingDef;
            if (thingDef != null)
            {
                Widgets.InfoCardButton(0f, curY, thingDef, stuffDef);
            }
            else
            {
                Widgets.InfoCardButton(0f, curY, entDef);
            }
            curY += 24f;
        }

        public void DebugSetStuffDef(ThingDef stuffDef)
        {
            this.stuffDef = stuffDef;
        }
    }
}