using System.Collections.Generic;

using UnityEngine;
using Verse;
using RimWorld;


namespace RA
{
    public class ITab_Container_Gear : ITab_Pawn_Gear
    {
        public string labelTitle;
        public CompContainer container;
        
        public ITab_Container_Gear()
        {
            labelKey = "Items";
        }
        public override bool IsVisible
        {
            get { return true; }
        }
        public string GetTitle()
        {
			//Stored items count: {0} / {1}
	        labelTitle = "Container_StoredItemsCount".Translate(container.ItemsCount, container.Properties.itemsCap);
            return labelTitle;
        }

        protected override void FillTab()
        {
            Building_Storage storage = this.SelThing as Building_Storage;
            this.container = storage.TryGetComp<CompContainer>();
            if (container == null)
            {
                Log.Error("No CompContainer included for this Building_Storage");
                return;
            }
            List<Thing> list = container.ListAllItems;
            float fieldHeight = 30.0f;
            this.size = new Vector2(300f, 55f + container.Properties.itemsCap * fieldHeight);

            Text.Font = GameFont.Small;

            Rect innerRect = GenUI.ContractedBy(new Rect(0.0f, 0.0f, this.size.x, this.size.y), 10f);
            float innerRectX = innerRect.x;
            GUI.BeginGroup(innerRect);
            {
                Widgets.TextField(new Rect(0.0f, 0.0f, this.size.x - 40f, fieldHeight), GetTitle());
                
                Rect thingIconRect = new Rect(10f, fieldHeight + 5f, 30f, fieldHeight);
                Rect thingLabelRect = new Rect(thingIconRect.x + 35f, thingIconRect.y + 5.0f, innerRect.width - 35f, fieldHeight);
                Rect thingButtonRect = new Rect(thingIconRect.x, thingIconRect.y, innerRect.width, fieldHeight);

                //float startY = 0.0f;
                //Widgets.ListSeparator(ref startY, innerRect.width, GetTitle());

                foreach (var thing in list)
                {
                    Widgets.ThingIcon(thingIconRect, thing);
                    Widgets.Label(thingLabelRect, thing.Label);

                    if (Widgets.InvisibleButton(thingButtonRect))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        options.Add(new FloatMenuOption("Container_Info".Translate(), () =>
                        {
                            // NOTE ?
                            Find.WindowStack.Add(new Dialog_InfoCard(thing));
                        }));
                        options.Add(new FloatMenuOption("Container_Drop".Translate(), () =>
                        {
                            IntVec3 bestSpot = IntVec3.Invalid;
                            if (JobDriver_HaulToCell.TryFindPlaceSpotNear(storage.Position, thing, out bestSpot))
                            {
                                thing.Position = bestSpot;
                            }
                            else
                            {
                                Log.Error("No free spot for " + thing);
                            }
                        }));

                        Find.WindowStack.Add(new FloatMenu(options, "", false, false));
                    }

                    thingIconRect.y += fieldHeight;
                    thingLabelRect.y += fieldHeight;
                    thingButtonRect.y += fieldHeight;
                }
            }
            GUI.EndGroup();
        }
    }
}
