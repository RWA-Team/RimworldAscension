using UnityEngine;
using Verse;

namespace RA
{
    public class ITab_Container : ITab
    {
        public static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        public static readonly Texture2D FullTexFuelCount = SolidColorMaterials.NewSolidColorTexture(new Color(0.6f, 0.6f, 0.6f));
        
        public Vector2 scrollPosition_Container = Vector2.zero;

        public ITab_Container()
        {
            labelKey = "Items";
        }
        public override bool IsVisible => true;

        protected override void FillTab()
        {
            UIUtil.ResetText();
            var storage = SelThing as Container;
            var compContainer = storage.TryGetComp<CompContainer>();
            size = new Vector2(250f, 55f + compContainer.Properties.itemsCap*UIUtil.TextHeight);

            var innerRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(UIUtil.DefaultMargin);

            // stored items count fillable bar
            var containerCapacityBarRect = new Rect(innerRect.x, innerRect.y, innerRect.width - UIUtil.CloseIconWidth, UIUtil.TextHeight);
            var containerCapacityPercent = storage.StoredItems.Count/(float)compContainer.Properties.itemsCap;
            Widgets.FillableBar(containerCapacityBarRect, containerCapacityPercent, FullTexFuelCount, EmptyTex, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(containerCapacityBarRect,
                string.Format("<b>Stored items count: {0}/{1}</b>", storage.StoredItems.Count, compContainer.Properties.itemsCap));

            var contentsRect =
                new Rect(innerRect.x, containerCapacityBarRect.yMax + UIUtil.DefaultMargin, innerRect.width,
                    innerRect.height - containerCapacityBarRect.yMax).ContractedBy(1f);
            UIUtil.DrawItemsList(contentsRect, ref scrollPosition_Container, storage.StoredItems, thing =>
            {
                Thing dummy;
                thing.DeSpawn();
                GenPlace.TryPlaceThing(thing, SelThing.RandomAdjacentCell8Way(), ThingPlaceMode.Near,
                    out dummy);
            });
            UIUtil.ResetText();
        }
    }
}
