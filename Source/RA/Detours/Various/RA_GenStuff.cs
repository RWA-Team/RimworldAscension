using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class RA_GenStuff
    {
        // assign first stuff type possible, instead of random
        public static ThingDef DefaultStuffFor(ThingDef def)
        {
            return !def.MadeFromStuff ? null : GenStuff.AllowedStuffsFor(def).FirstOrDefault();
        }

        //public static ThingDef DefaultStuffFor(ThingDef def)
        //{
        //    if (!def.MadeFromStuff)
        //    {
        //        return null;
        //    }

        //    if (def.Minifiable)
        //        return GenStuff.RandomStuffFor(def.minifiedDef);

        //    //// TODO: might cause performance issues (use resources readout?)
        //    //if (Current.ProgramState == ProgramState.MapPlaying)
        //    //{
        //    //    var existingResource =
        //    //        Find.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)?
        //    //            .FirstOrDefault(thing =>
        //    //                thing.Spawned && !thing.IsForbidden(Faction.OfPlayer) &&
        //    //                !Find.Reservations.IsReserved(thing, Faction.OfPlayer) && thing.def.IsStuff &&
        //    //                def.stuffProps.CanMake(def));

        //    //    return existingResource != null ? existingResource.Stuff : GenStuff.RandomStuffFor(def);
        //    //}

        //    return GenStuff.RandomStuffFor(def);
        //}
    }
}