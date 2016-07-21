using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RA
{
    public class Designator_PutInSlot : Designator
    {
        public static readonly Texture2D texPutInArrow = ContentFinder<Texture2D>.Get("UI/Gizmoes/PutIn");
        private static readonly Texture2D texOccupiedSlotBG = SolidColorMaterials.NewSolidColorTexture(1f, 1f, 1f, 0.1f);

        public CompSlots slotsComp;

        // overall gizmo width
        // Height = 75f
        public override float Width => slotsComp.Properties.maxSlots * Height + Height;

        public Designator_PutInSlot()
        {
            useMouseIcon = true;
            soundSucceeded = SoundDefOf.DesignateHaul;
            soundDragSustain = SoundDefOf.DesignateDragStandard;
            soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            activateSound = SoundDef.Named("Click");
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft)
        {
            var gizmoRect = new Rect(topLeft.x, topLeft.y, Width, Height);

            var designatorRect = new Rect(gizmoRect.x, gizmoRect.y, Height, Height);
            var result = DrawDesignator(designatorRect);
            GUI.DrawTexture(designatorRect, texPutInArrow);

            var inventoryRect = new Rect(gizmoRect.x + designatorRect.width, gizmoRect.y, Width - designatorRect.width, Height);
            Widgets.DrawWindowBackground(inventoryRect);
            DrawSlots(inventoryRect);

            return result;
        }

        // same as base.GizmoOnGUI(), but in specified rect size
        public GizmoResult DrawDesignator(Rect rect)
        {
            var mouseOver = false;
            if (Mouse.IsOver(rect))
            {
                mouseOver = true;
                GUI.color = GenUI.MouseoverColor;
            }
            GUI.DrawTexture(rect, BGTex);
            MouseoverSounds.DoRegion(rect, SoundDefOf.MouseoverCommand);

            // draw thing's texture
            Widgets.ThingIcon(rect, slotsComp.parent);

            var gizmoActivated = false;
            var keyCode = hotKey?.MainKey ?? KeyCode.None;
            if (keyCode != KeyCode.None && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
            {
                var rect2 = new Rect(rect.x + 5f, rect.y + 5f, 16f, 18f);
                Widgets.Label(rect2, keyCode.ToString());
                GizmoGridDrawer.drawnHotKeys.Add(keyCode);
                if (hotKey.KeyDownEvent)
                {
                    gizmoActivated = true;
                    Event.current.Use();
                }
            }
            if (Widgets.ButtonInvisible(rect))
            {
                gizmoActivated = true;
            }
            var labelCap = LabelCap;
            if (!labelCap.NullOrEmpty())
            {
                var num = Text.CalcHeight(labelCap, rect.width);
                num -= 2f;
                var rect3 = new Rect(rect.x, rect.yMax - num + 12f, rect.width, num);
                GUI.DrawTexture(rect3, TexUI.GrayTextBG);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect3, labelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            GUI.color = Color.white;
            if (DoTooltip)
            {
                TipSignal tip = Desc;
                if (disabled && !disabledReason.NullOrEmpty())
                {
                    tip.text = tip.text + "\n\nDISABLED: " + disabledReason;
                }
                TooltipHandler.TipRegion(rect, tip);
            }
            if (!tutorHighlightTag.NullOrEmpty())
            {
                TutorUIHighlighter.HighlightOpportunity(tutorHighlightTag, rect);
            }
            if (gizmoActivated)
            {
                if (disabled)
                {
                    if (!disabledReason.NullOrEmpty())
                    {
                        Messages.Message(disabledReason, MessageSound.RejectInput);
                    }
                    return new GizmoResult(GizmoState.Mouseover, null);
                }
                return new GizmoResult(GizmoState.Interacted, Event.current);
            }
            if (mouseOver)
            {
                return new GizmoResult(GizmoState.Mouseover, null);
            }
            return new GizmoResult(GizmoState.Clear, null);
        }

        public void DrawSlots(Rect inventoryRect)
        {
            // draw text message if no contents inside
            if (slotsComp.slots.Count == 0)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(inventoryRect, "No Items");
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Tiny;
            }
            // draw slots
            else
            {
                var slotRect = new Rect(inventoryRect.x, inventoryRect.y, Height, Height);
                for (var currentSlotInd = 0; currentSlotInd < slotsComp.Properties.maxSlots; currentSlotInd++)
                {
                    // draw occupied slots
                    if (currentSlotInd < slotsComp.slots.Count)
                    {
                        // draws greyish slot background
                        Widgets.DrawTextureFitted(slotRect.ContractedBy(3f), texOccupiedSlotBG, 1f);
                        // highlights slot if mouse over
                        Widgets.DrawHighlightIfMouseover(slotRect.ContractedBy(3f));

                        var currentThing = slotsComp.slots[currentSlotInd];

                        // draw thing texture
                        Widgets.ThingIcon(slotRect, currentThing);

                        // interaction with slots
                        if (Widgets.ButtonInvisible(slotRect))
                        {
                            // mouse button pressed
                            if (Event.current.button == 0)
                            {
                                // equip weapon in slot
                                if (currentThing.def.equipmentType == EquipmentType.Primary)
                                {
                                    slotsComp.SwapEquipment(currentThing as ThingWithComps);
                                }
                            }
                            // mouse button released
                            else if (Event.current.button == 1)
                            {
                                var options = new List<FloatMenuOption>
                                {
                                    new FloatMenuOption("Info",
                                        () => { Find.WindowStack.Add(new Dialog_InfoCard(currentThing)); }),
                                    new FloatMenuOption("Drop", () =>
                                    {
                                        Thing resultThing;
                                        slotsComp.slots.TryDrop(currentThing, slotsComp.owner.Position,
                                            ThingPlaceMode.Near, out resultThing);
                                    })
                                };
                                // get thing info card
                                // drop thing
                                Find.WindowStack.Add(new FloatMenu(options, currentThing.LabelCap));
                            }

                            // plays click sound on each click
                            SoundDefOf.Click.PlayOneShotOnCamera();
                        }
                    }

                    slotRect.x += Height;
                }
            }
        }

        // determine conditions of hiding similar gizmos
        public override bool GroupsWith(Gizmo other) { return false; }

        // allows rectangular selection
        public override int DraggableDimensions => 2;

        // draws number of selected objects
        public override bool DragDrawMeasurements => true;

        // returning string text assigning false reason to AcceptanceReport
        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            if (!cell.InBounds() || cell.Fogged() || cell.ContainsStaticFire())
            {
                return false;
            }

            var firstItem = cell.GetFirstItem();
            if (firstItem != null && CanDesignateThing(firstItem).Accepted)
            {
                return true;
            }

            return false;
        }

        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            // fast check if already added to "designations"
            if (slotsComp.designatedThings.Contains(thing))
            {
                return true;
            }

            // corpse has no icon\texture to render in the slot
            if (thing is Corpse)
            {
                return false;
            }

            if (!slotsComp.owner.CanReserveAndReach(thing, PathEndMode.ClosestTouch, slotsComp.owner.NormalMaxDanger()))
            {
                return false;
            }

            // if there are free slots
            if (slotsComp.designatedThings.Count + slotsComp.slots.Count >= slotsComp.Properties.maxSlots)
            {
                return false;
            }

            // if any of the thing's categories is allowed and not forbidden
            if (thing.def.thingCategories.Exists(category => slotsComp.Properties.allowedThingCategoryDefs.Exists(subCategory => subCategory.ThisAndChildCategoryDefs.Contains(category)) && !slotsComp.Properties.forbiddenSubThingCategoryDefs.Exists(subCategory => subCategory.ThisAndChildCategoryDefs.Contains(category))))
            {
                slotsComp.designatedThings.Add(thing);
                return true;
            }

            return false;
        }

        public override void DesignateSingleCell(IntVec3 cell)
        {
            DesignateThing(cell.GetFirstItem());
        }

        public override void DesignateThing(Thing thing)
        {
            // throws puffs to indicate that thigns were selected
            MoteThrower.ThrowMetaPuffs(thing);
        }

        // called when designator successfuly selects at least one thing
        protected override void FinalizeDesignationSucceeded()
        {
            // plays corresponding sound
            base.FinalizeDesignationSucceeded();

            var job = new Job(DefDatabase<JobDef>.GetNamed("PutInSlot"))
            {
                targetQueueA = new List<TargetInfo>(),
                numToBringList = new List<int>(),
                targetB = slotsComp.parent
            };

            foreach (var thing in slotsComp.designatedThings)
            {
                job.targetQueueA.Add(thing);
                job.numToBringList.Add(thing.def.stackLimit);
            }
            slotsComp.designatedThings.Clear();

            if (!job.targetQueueA.NullOrEmpty())
                slotsComp.owner.drafter.TakeOrderedJob(job);

            // remove active selection after click
            DesignatorManager.Deselect();
        }

        // draws selection brackets over designated things
        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}