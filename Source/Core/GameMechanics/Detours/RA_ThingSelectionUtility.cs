using Verse;

namespace RA
{
    public static class RA_ThingSelectionUtility
    {
        public static bool SelectableNow(this Thing t)
        {
            if (!t.def.selectable || !t.SpawnedInWorld || IsInContainer(t))
            {
                return false;
            }
            if (t.def.size.x == 1 && t.def.size.z == 1)
            {
                return !GridsUtility.Fogged(t.Position);
            }
            CellRect.CellRectIterator iterator = GenAdj.OccupiedRect(t).GetIterator();
            while (!iterator.Done())
            {
                if (!GridsUtility.Fogged(iterator.Current))
                {
                    return true;
                }
                iterator.MoveNext();
            }
            return false;
        }

        public static bool IsInContainer(Thing t)
        {
            Thing storage = Find.ThingGrid.ThingsListAtFast(t.Position).Find(thing => thing.TryGetComp<CompContainer>() != null);
            if (storage != null && t != storage)
            {
                return true;
            }
            return false;
        }
    }
}