using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class Container : Building_Storage
    {
        public CompContainer comp;

        public bool initialHide;

        // doesn't count container as thing
        public List<Thing> StoredItems => Find.ThingGrid.ThingsListAtFast(Position)
            .Where(thing =>
                thing != this &&
                thing.def.category == ThingCategory.Item)
            .ToList();

        // TODO: might hit performance
        public bool Full => Spawned && StoredItems.Count >= comp.Properties.itemsCap &&
                            !StoredItems.Exists(thing => stackCount < thing.def.stackLimit);

        // Executed when building is spawned on map (after loading too)
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            comp = this.TryGetComp<CompContainer>();
            if (comp == null)
            {
                Log.Error("No CompContainer included for this Container");
            }
        }

        // used instead of SpawnSetup, cause it's called after it, when all things registered in their cells
        public override void PostMapInit()
        {
            HideStoredItems();
        }

        public void HideStoredItems()
        {
            foreach (var item in StoredItems)
            {
                HideItem(item);
            }
        }

        public void HideItem(Thing thing)
        {
            // hides stored item texture
            Find.DynamicDrawManager.DeRegisterDrawable(thing);
            // hides stored item label
            ThingRequestGroup.HasGUIOverlay.ListByGroup().Remove(thing);
        }

        public override void DeSpawn()
        {
            ScatterItemsAround();

            base.DeSpawn();
        }

        // Scatter items to provide easy access
        public void ScatterItemsAround()
        {
            foreach (var thing in StoredItems)
            {
                Thing dummy;
                // despawn thing to spawn again with TryPlaceThing
                thing.DeSpawn();
                if (!GenPlace.TryPlaceThing(thing, thing.Position.RandomAdjacentCell8Way(), ThingPlaceMode.Near, out dummy))
                {
                    Log.Error("No free spot for " + thing);
                }
            }
        }

        // draw open\closed textures
        public override Graphic Graphic
        {
            get
            {
                if (Full)
                {
                    return GraphicDatabase.Get<Graphic_Single>(def.graphic.path + "_full", def.graphic.Shader,
                        def.graphic.drawSize, def.graphic.Color);
                }
                return base.Graphic;
            }
        }
    }
}
