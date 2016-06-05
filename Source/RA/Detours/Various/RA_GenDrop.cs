using Verse;
using Verse.Sound;

namespace RA
{
    public static class RA_GenDrop
    {
        // resets wasAutoEquipped value for picked up tools
        public static bool TryDropSpawn(Thing thing, IntVec3 dropCell, ThingPlaceMode mode, out Thing resultingThing)
        {
            if (!dropCell.InBounds())
            {
                Log.Error(string.Concat("Dropped ", thing, " out of bounds at ", dropCell));
                resultingThing = null;
                return false;
            }
            if (thing.def.destroyOnDrop)
            {
                thing.Destroy();
                resultingThing = null;
                return true;
            }
            thing.def.soundDrop?.PlayOneShot(dropCell);

            // tools injection
            var compTool = thing.TryGetComp<CompTool>();
            if (compTool != null) compTool.wasAutoEquipped = false;

            return GenPlace.TryPlaceThing(thing, dropCell, mode, out resultingThing);
        }
    }
}
