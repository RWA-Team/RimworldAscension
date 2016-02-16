using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace RA
{
    public abstract class Designator_Place : Designator
    {
        public const float RotButSize = 64f;

        public const float RotButSpacing = 10f;

        public Rot4 placingRot = Rot4.North;

        public static float middleMouseDownTime;

        public static readonly Texture2D RotLeftTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotLeft", true);

        public static readonly Texture2D RotRightTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight", true);

        public abstract BuildableDef PlacingDef
        {
            get;
        }

        public Designator_Place()
        {
            this.soundDragSustain = SoundDefOf.DesignateDragBuilding;
            this.soundDragChanged = SoundDefOf.DesignateDragBuildingChanged;
            this.soundSucceeded = SoundDefOf.DesignatePlaceBuilding;
        }

        //// determine conditions of hiding similar gizmos
        //public override bool GroupsWith(Gizmo other)
        //{
        //    Command command = other as Command;
        //    return command != null && ((this.hotKey == command.hotKey && this.defaultLabel == command.defaultLabel && this.Label == command.Label && this.icon == command.icon) || (this.groupKey != 0 && command.groupKey != 0 && this.groupKey == command.groupKey));
        //}


        public override void DoExtraGuiControls(float leftX, float bottomY)
        {
            ThingDef thingDef = this.PlacingDef as ThingDef;
            if (thingDef != null && thingDef.rotatable)
            {
                Rect winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
                this.HandleRotationShortcuts();
                Find.WindowStack.ImmediateWindow(73095, winRect, WindowLayer.GameUI, delegate
                {
                    RotationDirection rotationDirection = RotationDirection.None;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Medium;
                    Rect rect = new Rect(winRect.width / 2f - 64f - 5f, 15f, 64f, 64f);
                    if (Widgets.ImageButton(rect, Designator_Place.RotLeftTex))
                    {
                        SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                        rotationDirection = RotationDirection.Counterclockwise;
                        Event.current.Use();
                    }
                    Widgets.Label(rect, KeyBindingDefOf.DesignatorRotateLeft.MainKeyLabel);
                    Rect rect2 = new Rect(winRect.width / 2f + 5f, 15f, 64f, 64f);
                    if (Widgets.ImageButton(rect2, Designator_Place.RotRightTex))
                    {
                        SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                        rotationDirection = RotationDirection.Clockwise;
                        Event.current.Use();
                    }
                    Widgets.Label(rect2, KeyBindingDefOf.DesignatorRotateRight.MainKeyLabel);
                    if (rotationDirection != RotationDirection.None)
                    {
                        this.placingRot.Rotate(rotationDirection);
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                }, true, false, 1f);
            }
        }

        public override void SelectedUpdate()
        {
            GenDraw.DrawNoBuildEdgeLines();
            if (!ArchitectCategoryTab.InfoRect.Contains(GenUI.AbsMousePosition()))
            {
                IntVec3 intVec = Gen.MouseCell();
                if (this.PlacingDef is TerrainDef)
                {
                    GenUI.RenderMouseoverBracket();
                    return;
                }
                Color ghostCol;
                if (this.CanDesignateCell(intVec).Accepted)
                {
                    ghostCol = new Color(0.5f, 1f, 0.6f, 0.4f);
                }
                else
                {
                    ghostCol = new Color(1f, 0f, 0f, 0.4f);
                }
                this.DrawGhost(ghostCol);
                if (this.CanDesignateCell(intVec).Accepted && this.PlacingDef.specialDisplayRadius > 0.01f)
                {
                    if (this.PlacingDef.defName.Contains("TradingPost"))
                    {
                        GenDraw.DrawFieldEdges(TradeableCells.ToList());
                    }
                    else
                        GenDraw.DrawRadiusRing(Gen.MouseCell(), this.PlacingDef.specialDisplayRadius);
                }
                GenDraw.DrawInteractionCell((ThingDef)this.PlacingDef, intVec, this.placingRot);
            }
        }

        public IEnumerable<IntVec3> TradeableCells
        {
            get
            {
                // half width of the rectangle, determined by <specialDisplayRadius> in building def
                int TradeRectangleRange = (int)this.PlacingDef.specialDisplayRadius + 1;

                IntVec3 centerCell = Gen.MouseCell();
                IntVec3 currentCell = centerCell;

                for (int i = centerCell.x - TradeRectangleRange; i < centerCell.x + TradeRectangleRange + 1; i++)
                {
                    currentCell.x = i;
                    for (int j = centerCell.z - TradeRectangleRange; j < centerCell.z + TradeRectangleRange + 1; j++)
                    {
                        currentCell.z = j;
                        if ((Math.Abs(centerCell.x - currentCell.x) > 1 || Math.Abs(centerCell.z - currentCell.z) > 1) && GenGrid.InBounds(currentCell) && currentCell.Walkable())
                            yield return currentCell;
                    }
                }
            }
        }

        public virtual void DrawGhost(Color ghostCol)
        {
            GhostDrawer.DrawGhostThing(Gen.MouseCell(), this.placingRot, (ThingDef)this.PlacingDef, null, ghostCol, AltitudeLayer.Blueprint);
        }

        public void HandleRotationShortcuts()
        {
            RotationDirection rotationDirection = RotationDirection.None;
            if (Event.current.button == 2)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                    Designator_Place.middleMouseDownTime = Time.realtimeSinceStartup;
                }
                if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - Designator_Place.middleMouseDownTime < 0.15f)
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
                this.placingRot.Rotate(RotationDirection.Clockwise);
            }
            if (rotationDirection == RotationDirection.Counterclockwise)
            {
                SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                this.placingRot.Rotate(RotationDirection.Counterclockwise);
            }
        }
    }
}
