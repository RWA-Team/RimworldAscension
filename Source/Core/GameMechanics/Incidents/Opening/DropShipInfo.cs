using System.Collections.Generic;
using Verse;

namespace RA
{
    public class DropShipInfo : IExposable
    {
        public List<Thing> containedThings = new List<Thing>(); // Ship content and config
        public int openDelay = 110; // Delay before dropping contents

        public Thing SingleContainedThing
        {
            get
            {
                // Return a single thing from contents
                if (this.containedThings.Count == 0)
                {
                    return null;
                }
                if (this.containedThings.Count > 1)
                {
                    Log.Error("ContainedThing used on a DropPodCrashingInfo holding > 1 thing.");
                }
                return this.containedThings[0];
            }
            set
            {
                // Setting contents so clear list
                this.containedThings.Clear();
                // And add what was passed
                this.containedThings.Add(value);
            }
        }

        public void ExposeData()
        {
            // Save contents to savefile
            Scribe_Collections.LookList<Thing>(ref this.containedThings, "containedThings", LookMode.Deep, new object[0]);
            // Save open delay to save file
            Scribe_Values.LookValue<int>(ref this.openDelay, "openDelay", openDelay, false);
        }
    }
}
