using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    // tries to assign existing stuff type, instead of some random one, as default
    public static class RA_GenStuff
    {
        public static ThingDef DefaultStuffFor(ThingDef def)
        {
            if (Current.ProgramState != ProgramState.MapPlaying || !def.MadeFromStuff)
                return null;

            //if (def.Minifiable)
            //    return GenStuff.DefaultStuffFor(def.minifiedDef);

            // TODO: might cause performance issues (use resources readout?)
            var existingResource =
                Find.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                    .FirstOrDefault(thing =>
                        thing.Spawned && !thing.IsForbidden(Faction.OfPlayer) && !Find.Reservations.IsReserved(thing,Faction.OfPlayer) && thing.def.IsStuff &&
                        def.stuffProps.CanMake(def));

            return existingResource != null ? existingResource.Stuff : GenStuff.RandomStuffFor(def);
        }
    }
}