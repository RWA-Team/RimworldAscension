using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace RA
{
    public class Designator_PutInSlot : Designator
    {
        public static readonly Texture2D texPutInArrow = ContentFinder<Texture2D>.Get("UI/Icons/PutIn");
        private static readonly Texture2D texOccupiedSlotBG = SolidColorMaterials.NewSolidColorTexture(1f, 1f, 1f, 0.1f);

        public CompSlots slotsComp;

        // overall gizmo width
        // Height = 75f
        public override float Width { get { return slotsComp.Properties.maxSlots * Height + Height; } }

        public Designator_PutInSlot()
        {
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.DesignateHaul;
            this.soundDragSustain = SoundDefOf.DesignateDragStandard;
            this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            this.activateSound = SoundDef.Named("Click");
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft)
        {
            Rect gizmoRect = new Rect(topLeft.x, topLeft.y, Width, Height);

            Rect designatorRect = new Rect(gizmoRect.x, gizmoRect.y, Height, Height);
            GizmoResult result = DrawDesignator(designatorRect);
            GUI.DrawTexture(designatorRect, texPutInArrow);

            Rect inventoryRect = new Rect(gizmoRect.x + designatorRect.width, gizmoRect.y, Width - designatorRect.width, Height);
            Widgets.DrawWindowBackground(inventoryRect);
            DrawSlots(inventoryRect);

            return result;
        }

        // same as base.GizmoOnGUI(), but in specified rect size
        public GizmoResult DrawDesignator(Rect rect)
        {
            bool mouseOver = false;
            if (Mouse.IsOver(rect))
            {
                mouseOver = true;
                GUI.color = GenUI.MouseoverColor;
            }
            GUI.DrawTexture(rect, this.BGTex);
            MouseoverSounds.DoRegion(rect, SoundDefOf.MouseoverButtonCommand);

            // draw thing's texture
            Widgets.ThingIcon(rect, slotsComp.parent);

            bool gizmoActivated = false;
            KeyCode keyCode = (this.hotKey != null) ? this.hotKey.MainKey : KeyCode.None;
            if (keyCode != KeyCode.None && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
            {
                Rect rect2 = new Rect(rect.x + 5f, rect.y + 5f, 16f, 18f);
                Widgets.Label(rect2, keyCode.ToString());
                GizmoGridDrawer.drawnHotKeys.Add(keyCode);
                if (this.hotKey.KeyDownEvent)
                {
                    gizmoActivated = true;
                    Event.current.Use();
                }
            }
            if (Widgets.InvisibleButton(rect))
            {
                gizmoActivated = true;
            }
            string labelCap = this.LabelCap;
            if (!labelCap.NullOrEmpty())
            {
                float num = Text.CalcHeight(labelCap, rect.width);
                num -= 2f;
                Rect rect3 = new Rect(rect.x, rect.yMax - num + 12f, rect.width, num);
                GUI.DrawTexture(rect3, TexUI.GrayTextBG);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect3, labelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            GUI.color = Color.white;
            if (this.DoTooltip)
            {
                TipSignal tip = this.Desc;
                if (this.disabled && !this.disabledReason.NullOrEmpty())
                {
                    tip.text = tip.text + "\n\nDISABLED: " + this.disabledReason;
                }
                TooltipHandler.TipRegion(rect, tip);
            }
            if (!this.tutorHighlightTag.NullOrEmpty())
            {
                TutorUIHighlighter.HighlightOpportunity(this.tutorHighlightTag, rect);
            }
            if (gizmoActivated)
            {
                if (this.disabled)
                {
                    if (!this.disabledReason.NullOrEmpty())
                    {
                        Messages.Message(this.disabledReason, MessageSound.RejectInput);
                    }
                    return new GizmoResult(GizmoState.Mouseover, null);
                }
                return new GizmoResult(GizmoState.Interacted, Event.current);
            }
            else
            {
                if (mouseOver)
                {
                    return new GizmoResult(GizmoState.Mouseover, null);
                }
                return new GizmoResult(GizmoState.Clear, null);
            }
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
                Rect slotRect = new Rect(inventoryRect.x, inventoryRect.y, Height, Height);
                for (int currentSlotInd = 0; currentSlotInd < slotsComp.Properties.maxSlots; currentSlotInd++)
                {
                    // draw occupied slots
                    if (currentSlotInd < slotsComp.slots.Count)
                    {
                        // draws greyish slot background
                        Widgets.DrawTextureFitted(slotRect.ContractedBy(3f), texOccupiedSlotBG, 1f);
                        // highlights slot if mouse over
                        Widgets.DrawHighlightIfMouseover(slotRect.ContractedBy(3f));

                        Thing currentThing = slotsComp.slots[currentSlotInd];

                        // draw thing texture
                        Widgets.ThingIcon(slotRect, currentThing);

                        // interaction with slots
                        if (Widgets.InvisibleButton(slotRect))
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
                                List<FloatMenuOption> options = new List<FloatMenuOption>();
                                // get thing info card
                                options.Add(new FloatMenuOption("Info", () =>
                                {
                                    Find.WindowStack.Add((Window)new Dialog_InfoCard(currentThing));
                                }));
                                // drop thing
                                options.Add(new FloatMenuOption("Drop", () =>
                                {
                                    Thing resultThing;
                                    slotsComp.slots.TryDrop(currentThing, slotsComp.owner.Position, ThingPlaceMode.Near, out resultThing);
                                }));
                                Find.WindowStack.Add((Window)new FloatMenu(options, currentThing.LabelCap, false, false));
                            }

                            // plays click sound on each click
                            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                        }
                    }

                    slotRect.x += Height;
                }
            }
        }

        // determine conditions of hiding similar gizmos
        public override bool GroupsWith(Gizmo other) { return false; }

        // allows rectangular selection
        public override int DraggableDimensions { get { return 2; } }

        // returning string text assigning false reason to AcceptanceReport
        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            if (!cell.InBounds() || cell.Fogged() || cell.ContainsStaticFire())
            {
                return false;
            }

            Thing firstItem = cell.GetFirstItem();
            if (firstItem != null && this.CanDesignateThing(firstItem).Accepted)
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

            Job job = new Job(DefDatabase<JobDef>.GetNamed("PutInSlot"))
            {
                targetQueueA = new List<TargetInfo>(),
                numToBringList = new List<int>(),
                targetB = slotsComp.parent
            };

            foreach (Thing thing in slotsComp.designatedThings)
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