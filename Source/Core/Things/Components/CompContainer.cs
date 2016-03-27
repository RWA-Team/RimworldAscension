using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class CompContainer : ThingComp
    {
        public Dictionary<Thing, RottableData> rottablesList = new Dictionary<Thing, RottableData>();

        public List<Thing> hiddenItems = new List<Thing>();
        public List<Thing> tempBuffer = new List<Thing>();
        public List<Thing> thingsWithLabels = new List<Thing>();

        public List<Thing> ListAllItems { get { return Find.ThingGrid.ThingsListAtFast(parent.Position).Where(thing => thing != parent).ToList(); } }
        // doesn't count container as thing
        public int ItemsCount => ListAllItems.Count;

        public CompContainer_Properties Properties => (CompContainer_Properties)props;

        // Executed when building is spawned on map (after loading too)
        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();

            // Restrict itemsCap to prevent unsafe behaviour
            if (Properties.itemsCap < 1 || Properties.itemsCap > 15)
            {
                Properties.itemsCap = 10;
                Log.Error("CompContainer's itemsCap value should be between 1 and 15");
            }
        }

        // hide all intems "inside" container and show them again if removed
        public override void CompTick()
        {
            // NOTE: performance hit? (if perfomed each tick)

            thingsWithLabels = ThingRequestGroup.HasGUIOverlay.ListByGroup();

            HideStoredItems();
            DrawRemovedItems();

            // individual interval of 250 ticks
            if (parent.IsHashIntervalTick(GenTicks.TickRareInterval))
                AdjustRottables();
        }

        public void HideStoredItems()
        {
            foreach (var item in ListAllItems)
            {
                if (!hiddenItems.Contains(item))
                {
                    Find.DynamicDrawManager.DeRegisterDrawable(item);
                    hiddenItems.Add(item);
                    thingsWithLabels.Remove(item);
                }
            }
        }
        
        public void DrawRemovedItems()
        {
            tempBuffer.Clear();
            foreach (var item in hiddenItems)
            {
                if (!ListAllItems.Contains(item))
                {
                    Find.DynamicDrawManager.RegisterDrawable(item);
                    tempBuffer.Add(item);
                    thingsWithLabels.Add(item);
                }
            }
            hiddenItems = hiddenItems.Except(tempBuffer).ToList();
        }

        public void AdjustRottables()
        {
            if (Properties.rotModifier != 1f)
            {
                foreach (var item in hiddenItems)
                {
                    CompRottable compRot;
                    if ((compRot = item.TryGetComp<CompRottable>()) != null)
                    {
                        // if rottable data already saved, regenerate
                        if (rottablesList.ContainsKey(item))
                        {
                            // adjust rot progress according to rotModifier
                            item.HitPoints = Mathf.RoundToInt(item.HitPoints + (item.HitPoints - rottablesList[item].HitPoints) * Properties.rotModifier);
                            compRot.rotProgress += (compRot.rotProgress - rottablesList[item].rotProgress) * Properties.rotModifier;
                        }
                        // if just added, save data
                        else
                            rottablesList.Add(item, new RottableData(item.HitPoints, compRot.rotProgress));
                    }
                }
            }
        }

        public override void PostDestroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDestroy(mode);

            ScatterItemsAround();
            DrawRemovedItems();
        }

        // Scatter items to provide easy access
        public void ScatterItemsAround()
        {
            foreach (var thing in ListAllItems)
            {
                IntVec3 bestSpot;
                if (JobDriver_HaulToCell.TryFindPlaceSpotNear(parent.Position, thing, out bestSpot))
                {
                    thing.Position = bestSpot;
                }
                else
                {
                    Log.Error("No free spot for " + thing);
                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Collections.LookList(ref hiddenItems, "listHiddenItems", LookMode.Deep);
            Scribe_Collections.LookDictionary(ref rottablesList, "rottablesList", LookMode.MapReference, LookMode.Deep);
        }
    }

    public class CompContainer_Properties : CompProperties
    {
        // Default value
        public int itemsCap = 10;
        public float rotModifier = 1f;

        // Default requirement
        public CompContainer_Properties()
        {
            compClass = typeof(CompContainer);
        }
    }

    public class RottableData : IExposable
    {
        public int HitPoints;
        public float rotProgress;

        // required for exposing
        public RottableData() { }

        public RottableData(int HitPoints, float rotProgress)
        {
            this.HitPoints = HitPoints;
            this.rotProgress = rotProgress;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref HitPoints, "HitPoints");
            Scribe_Values.LookValue(ref rotProgress, "rotProgress");
        }
    }
}