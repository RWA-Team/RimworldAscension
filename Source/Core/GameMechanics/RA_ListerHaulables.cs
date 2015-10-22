using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace RA
{
    // Partial vanilla source class without non used methods
    public static class RA_ListerHaulables
    {
        public static List<Thing> haulables = new List<Thing>();

        public const int CellsPerTick = 4;
        public static int groupCycleIndex = 0;
        public static List<int> cellCycleIndices = new List<int>();

        public static void ListerHaulablesTick()
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

            int groupInd = groupCycleIndex % sgList.Count;
            var sg = sgList[groupCycleIndex % sgList.Count];

            while (cellCycleIndices.Count <= groupInd)
            {
                cellCycleIndices.Add(0);
            }
            if (cellCycleIndices[groupInd] >= int.MaxValue - 10000)
                cellCycleIndices[groupInd] = 0;
            for (int i = 0; i < CellsPerTick; i++)
            {
                cellCycleIndices[groupInd]++;

                var cell = sg.CellsList[cellCycleIndices[groupInd] % sg.CellsList.Count];

                var thingList = cell.GetThingList();
                for (int j = 0; j < thingList.Count; j++)
                {
                    if (thingList[j].def.EverHaulable)
                    {
                        Check(thingList[j]);

                        break;
                    }
                }
            }
        }

        public static void RecalcAllInCell(IntVec3 c)
        {
            var list = c.GetThingList();
            for (int i = 0; i < list.Count; i++)
            {
                Check(list[i]);
            }
        }

        public static void Check(Thing t)
        {
            if (WorkGiver_HaulGeneral.ShouldBeHaulable(t))
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
    }
}