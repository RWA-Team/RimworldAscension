using Verse;

namespace RA
{
    class MapCompColonyHistory : MapComponent
    {
        public float ShakeDecayRate = 0.5f;

        public override void ExposeData()
        {
            base.ExposeData();

            //Scribe_Deep.LookDeep(ref colonyOffer, "colonyOffer", new object[] { this });
            //Scribe_Collections.LookList(ref merchantOffer, "merchantOffer", LookMode.Deep, new object[0]);
            //Scribe_Collections.LookList(ref offeredItems, "offeredItems", LookMode.Deep, new object[0]);
            //Scribe_Collections.LookList(ref offeredResourceCounters, "offeredResourceCounters", LookMode.Deep, new object[] { this });
            //Scribe_Deep.LookDeep(ref merchant, "merchant", new object[0]);
            //Scribe_Values.LookValue(ref occupied, "occupied");
        }
    }
}
