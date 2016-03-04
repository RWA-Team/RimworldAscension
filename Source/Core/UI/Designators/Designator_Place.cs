using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public abstract class Designator_Place : Designator
    {
        public const float RotButSize = 64f;
        public const float RotButSpacing = 10f;

        public static readonly Texture2D RotLeftTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotLeft");
        public static readonly Texture2D RotRightTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight");

        public static Dictionary<int, Graphic> ghostGraphics = new Dictionary<int, Graphic>();

        public Rot4 placingRot = Rot4.North;
        public static float middleMouseDownTime;
        public ThingDef stuffDef = null;

        public abstract BuildableDef PlacingDef
        {
            get;
        }

        protected Designator_Place()
        {
            soundDragSustain = SoundDefOf.DesignateDragBuilding;
            soundDragChanged = SoundDefOf.DesignateDragBuildingChanged;
            soundSucceeded = SoundDefOf.DesignatePlaceBuilding;
        }

        public override void DoExtraGuiControls(float leftX, float bottomY)
        {
            var thingDef = PlacingDef as ThingDef;
            if (thingDef != null && thingDef.rotatable)
            {
                var winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
                HandleRotationShortcuts();
                Find.WindowStack.ImmediateWindow(73095, winRect, WindowLayer.GameUI, delegate
                {
                    var rotationDirection = RotationDirection.None;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Medium;
                    var rect = new Rect(winRect.width / 2f - 64f - 5f, 15f, 64f, 64f);
                    if (Widgets.ImageButton(rect, RotLeftTex))
                    {
                        SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                        rotationDirection = RotationDirection.Counterclockwise;
                        Event.current.Use();
                    }
                    Widgets.Label(rect, KeyBindingDefOf.DesignatorRotateLeft.MainKeyLabel);
                    var rect2 = new Rect(winRect.width / 2f + 5f, 15f, 64f, 64f);
                    if (Widgets.ImageButton(rect2, RotRightTex))
                    {
                        SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                        rotationDirection = RotationDirection.Clockwise;
                        Event.current.Use();
                    }
                    Widgets.Label(rect2, KeyBindingDefOf.DesignatorRotateRight.MainKeyLabel);
                    if (rotationDirection != RotationDirection.None)
                    {
                        placingRot.Rotate(rotationDirection);
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                });
            }
        }

        public override void SelectedUpdate()
        {
            GenDraw.DrawNoBuildEdgeLines();
            if (!ArchitectCategoryTab.InfoRect.Contains(GenUI.AbsMousePosition()))
            {
                var intVec = Gen.MouseCell();
                if (PlacingDef is TerrainDef)
                {
                    GenUI.RenderMouseoverBracket();
                    return;
                }

                var ghostCol = CanDesignateCell(intVec).Accepted ? new Color(0.5f, 1f, 0.6f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);

                // special Graphic_StuffBased implementation
                var baseGraphic = PlacingDef.graphic as Graphic_StuffBased;
                if (baseGraphic!=null)
                    baseGraphic.currentCategory = stuffDef.stuffProps.categories[0].defName;

                GhostDrawer.DrawGhostThing(Gen.MouseCell(), placingRot, (ThingDef)PlacingDef, baseGraphic?.categorizedGraphics[baseGraphic.currentCategory], ghostCol, AltitudeLayer.Blueprint);

                if (CanDesignateCell(intVec).Accepted && PlacingDef.specialDisplayRadius > 0.01f)
                {
                    if (PlacingDef.defName.Contains("TradingPost"))
                    {
                        GenDraw.DrawFieldEdges(TradeableCells.ToList());
                    }
                    else
                        GenDraw.DrawRadiusRing(Gen.MouseCell(), PlacingDef.specialDisplayRadius);
                }
                GenDraw.DrawInteractionCell((ThingDef)PlacingDef, intVec, placingRot);
            }
        }

        public IEnumerable<IntVec3> TradeableCells
        {
            get
            {
                // half width of the rectangle, determined by <specialDisplayRadius> in building def
                var TradeRectangleRange = (int)PlacingDef.specialDisplayRadius + 1;

                var centerCell = Gen.MouseCell();
                var currentCell = centerCell;

                for (var i = centerCell.x - TradeRectangleRange; i < centerCell.x + TradeRectangleRange + 1; i++)
                {
                    currentCell.x = i;
                    for (var j = centerCell.z - TradeRectangleRange; j < centerCell.z + TradeRectangleRange + 1; j++)
                    {
                        currentCell.z = j;
                        if ((Math.Abs(centerCell.x - currentCell.x) > 1 || Math.Abs(centerCell.z - currentCell.z) > 1) && currentCell.InBounds() && currentCell.Walkable())
                            yield return currentCell;
                    }
                }
            }
        }

        public void HandleRotationShortcuts()
        {
            var rotationDirection = RotationDirection.None;
            if (Event.current.button == 2)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                    middleMouseDownTime = Time.realtimeSinceStartup;
                }
                if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - middleMouseDownTime < 0.15f)
                {
                    rotationDirection = RotationDirection.Clockwise;
                }
            }
            if (KeyBindingDefOf.DesignatorRotateRight.KeyDownEvent)
            {
                rotationDirection = RotationDirection.Clockwise;
            }
            if (KeyBindingDefOf.DesignatorRotateLeft.KeyDownEvent)
            {
                rotationDirection = RotationDirection.Counterclockwise;
            }
            if (rotationDirection == RotationDirection.Clockwise)
            {
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                placingRot.Rotate(RotationDirection.Clockwise);
            }
            if (rotationDirection == RotationDirection.Counterclockwise)
            {
                SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                placingRot.Rotate(RotationDirection.Counterclockwise);
            }
        }
    }
}
