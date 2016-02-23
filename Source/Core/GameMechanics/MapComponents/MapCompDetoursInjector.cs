using System.Reflection;

using Verse;
using RimWorld;

namespace RA
{
    public class MapCompDetoursInjector : MapComponent
    {
        public MapCompDetoursInjector()
        {
            // Detour RimWorld.ThingSelectionUtility.SelectableNow
            MethodInfo vanillaSelectableNow = typeof(RimWorld.ThingSelectionUtility).GetMethod("SelectableNow", BindingFlags.Static | BindingFlags.Public);
            MethodInfo newSelectableNow = typeof(ThingSelectionUtility).GetMethod("SelectableNow", BindingFlags.Static | BindingFlags.NonPublic);
            Detours.TryDetourFromTo(vanillaSelectableNow, newSelectableNow);

            //// Detour RimWorld.WorkGiver_DoBill.ThingIsUsableBillGiver
            //MethodInfo vanillaThingIsUsableBillGiver = typeof(RimWorld.WorkGiver_DoBill).GetMethod("ThingIsUsableBillGiver", BindingFlags.NonPublic);
            //MethodInfo newThingIsUsableBillGiver = typeof(WorkGiver_DoBill).GetMethod("ThingIsUsableBillGiver", BindingFlags.NonPublic);
            //Detours.TryDetourFromTo(vanillaThingIsUsableBillGiver, newThingIsUsableBillGiver);
        }
    }

    #region REPLACEMENT CLASSES

    internal static class ThingSelectionUtility
    {
        internal static bool SelectableNow(this Thing t)
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

    //internal static class WorkGiver_DoBill
    //{
    //    internal static bool ThingIsUsableBillGiver(this WorkGiverDef def, Thing thing)
    //    {
    //        Pawn pawn = thing as Pawn;
    //        Corpse corpse = thing as Corpse;
    //        Pawn pawn2 = null;
    //        if (corpse != null)
    //        {
    //            pawn2 = corpse.innerPawn;
    //        }
    //        if (thing.def == def.singleBillGiverDef || ThingRequestGroup.PotentialBillGiver.Includes(thing.def))
    //        {
    //            return true;
    //        }
    //        if (pawn != null)
    //        {
    //            if (def.billGiversAllHumanlikes && pawn.RaceProps.Humanlike)
    //            {
    //                return true;
    //            }
    //            if (def.billGiversAllMechanoids && pawn.RaceProps.mechanoid)
    //            {
    //                return true;
    //            }
    //            if (def.billGiversAllAnimals && pawn.RaceProps.Animal)
    //            {
    //                return true;
    //            }
    //        }
    //        if (corpse != null && pawn2 != null)
    //        {
    //            if (def.billGiversAllHumanlikesCorpses && pawn2.RaceProps.Humanlike)
    //            {
    //                return true;
    //            }
    //            if (def.billGiversAllMechanoidsCorpses && pawn2.RaceProps.mechanoid)
    //            {
    //                return true;
    //            }
    //            if (def.billGiversAllAnimalsCorpses && pawn2.RaceProps.Animal)
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }
    //}

    #endregion
}
