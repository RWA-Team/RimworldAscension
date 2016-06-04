using Verse;

namespace RA
{
    public static class RA_ThingSelectionUtility
    {
        public static bool SelectableNow(this Thing t)
        {
            if (!t.def.selectable || !t.Spawned || IsInContainer(t))
            {
                return false;
            }
            if (t.def.size.x == 1 && t.def.size.z == 1)
            {
                return !t.Position.Fogged();
            }
            var iterator = t.OccupiedRect().GetIterator();
            while (!iterator.Done())
            {
                if (!iterator.Current.Fogged())
                {
                    return true;
                }
                iterator.MoveNext();
            }
            return false;
        }

        public static bool IsInContainer(Thing t)
        {
            var storage = Find.ThingGrid.ThingsListAtFast(t.Position).Find(thing => thing.TryGetComp<CompContainer>() != null);
            if (storage != null && t != storage)
            {
                return true;
            }
            return false;
        }
    }
}