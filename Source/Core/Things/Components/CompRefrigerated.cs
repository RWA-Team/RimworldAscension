using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class CompRefrigerated : ThingComp
    {
        public Dictionary<Thing, RottableData> rottablesList = new Dictionary<Thing, RottableData>();

        public List<Thing> itemsList { get { return Find.ThingGrid.ThingsListAtFast(parent.Position).Where(thing => thing.def.thingClass != typeof(Building_Storage)).ToList(); } }

        // Executed when building is spawned on map (after loading too)
        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();

            if (parent.TryGetComp<CompPowerTrader>() == null)
                Log.Error(parent.def.defName + " require CompPowerTrader component in it's def");

            RefrigerateContents();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            
            RefrigerateContents();
        }

        public void RefrigerateContents()
        {
            // Only refrigerate if it has power
            if (parent.TryGetComp<CompPowerTrader>().PowerOn == false)
            {
                return;
            }

            // Look for things
            ScanForRottables();

            CompRottable compRottable;
            // Refrigerate the items
            foreach (Thing item in itemsList)
            {
                compRottable = item.TryGetComp<CompRottable>();
                if (compRottable != null)
                {
                    // reset to saved values;
                    item.HitPoints = rottablesList[item].HitPoints;
                    compRottable.rotProgress = rottablesList[item].rotProgress;
                }
            }
        }

        public void ScanForRottables()
        {
            List<Thing> changesList = new List<Thing>();
            // Things to remove from rottables list
            foreach (KeyValuePair<Thing, RottableData> item in rottablesList)
            {
                if (!itemsList.Contains(item.Key))
                {
                    changesList.Add(item.Key);
                }
            }

            foreach (Thing thing in changesList)
                rottablesList.Remove(thing);
            changesList.Clear();

            // Things to add to rottables list
            foreach (Thing thing in itemsList)
            {
                if (thing.TryGetComp<CompRottable>() != null && !rottablesList.ContainsKey(thing))
                {
                    changesList.Add(thing);
                }
            }

            foreach (Thing thing in changesList)
                rottablesList.Add(thing, new RottableData(thing.HitPoints, thing.TryGetComp<CompRottable>().rotProgress));
        }
    }

    public class RottableData
    {
        public int HitPoints;
        public float rotProgress;

        public RottableData(int HitPoints, float rotProgress)
        {
            this.HitPoints = HitPoints;
            this.rotProgress = rotProgress;
        }
    }
}