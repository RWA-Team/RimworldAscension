using System.Collections.Generic;
using Verse;

namespace RA
{
    public class MapCompDataStorage : MapComponent
    {
        // reserved weapons to swap with tool or initially carried weapon later
        public static Dictionary<ThingWithComps, Pawn> previousPawnWeapons = new Dictionary<ThingWithComps, Pawn>();

        public override void ExposeData()
        {
            Scribe_Collections.LookDictionary(ref previousPawnWeapons, "previousPawnWeapons", LookMode.MapReference,
                LookMode.MapReference);
        }
    }
}
