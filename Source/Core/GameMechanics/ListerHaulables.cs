using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    // Partial vanilla source class without non used methods
    public static class ListerHaulables
    {
        public static List<Thing> haulables = new List<Thing>();
        public static int previousHaulablesCount_Vanilla = RimWorld.ListerHaulables.ThingsPotentiallyNeedingHauling().Count;

        public const int CellsPerTick = 4;
        public static int groupCycleIndex;
        public static List<int> cellCycleIndices = new List<int>();

        //Find things that require hauling in storages
        public static void FindHaulablesInSlotGroups()
        {
            /*  This is how we detect when relative SlotGroup priorities or storage settings
             * have changed to make something relevant to haul.
             * 
             * Each tick, we look at a few EverHaulables who are already in SlotGroups.
             * We look at all SlotGroups of higher priority than their current one.
             * Go through their cells. As soon as a cell can accept the thing, we add it to the haulable list.
             * If none are found, we remove it from the haulable list.
             * */

            groupCycleIndex++;
            if (groupCycleIndex >= int.MaxValue - 10000)
                groupCycleIndex = 0;

            var sgList = Find.SlotGroupManager.AllGroupsListForReading;

            if (sgList.Count == 0)
                return;

            var groupInd = groupCycleIndex % sgList.Count;
            var sg = sgList[groupCycleIndex % sgList.Count];

            while (cellCycleIndices.Count <= groupInd)
            {
                cellCycleIndices.Add(0);
            }
            if (cellCycleIndices[groupInd] >= int.MaxValue - 10000)
                cellCycleIndices[groupInd] = 0;
            for (var i = 0; i < CellsPerTick; i++)
            {
                cellCycleIndices[groupInd]++;

                var cell = sg.CellsList[cellCycleIndices[groupInd] % sg.CellsList.Count];

                var thingList = cell.GetThingList();
                foreach (Thing thing in thingList)
                {
                    if (thing.def.EverHaulable)
                    {
                        CheckIfHaulable(thing);

                        break;
                    }
                }
            }
        }

        //Find things that require hauling in container's stacks
        public static void FindHaulablesInContainers()
        {
            //Storage Buildings (like hoppers), which have CompContainer
            var containersList = Find.ListerBuildings.AllBuildingsColonistOfClass<Building_Storage>().Where(building => building.TryGetComp<CompContainer>() != null).ToList();

            //Checking things that require hauling in each container and adding them to haulables list
            if (containersList.Count > 0)
            {
                foreach (var container in containersList)
                {
                    foreach (var item in container.TryGetComp<CompContainer>().ListAllItems)
                    {
                        CheckIfHaulable(item);
                    }
                }
            }
        }

        public static void RecalcAllInCell(IntVec3 c)
        {
            var list = c.GetThingList();
            foreach (Thing thing in list)
            {
                CheckIfHaulable(thing);
            }
        }

        public static void CheckIfHaulable(Thing t)
        {
            if (ShouldBeHaulable(t))
            {
                if (!haulables.Contains(t))
                    haulables.Add(t);
            }
            else
            {
                if (haulables.Contains(t))
                    haulables.Remove(t);
            }
        }

        // duplicated to make changes
        public static bool ShouldBeHaulable(Thing t)
        {
            return (t.def.alwaysHaulable || (t.def.EverHaulable && (Find.DesignationManager.DesignationOn(t, DesignationDefOf.Haul) != null || t.IsInAnyStorage()))) && !t.IsForbidden(Faction.OfColony) && !IsInValidBestStorage(t);
        }

        // duplicated to make changes
        public static bool IsInValidBestStorage(Thing t)
        {
            var slotGroup = t.Position.GetSlotGroup();
            IntVec3 intVec;
            return slotGroup != null && slotGroup.Settings.AllowedToAccept(t) && !WorkGiver_HaulGeneral.TryFindBestBetterStoreCellFor(t, null, slotGroup.Settings.Priority, Faction.OfColony, out intVec, false);
        }
    }
}