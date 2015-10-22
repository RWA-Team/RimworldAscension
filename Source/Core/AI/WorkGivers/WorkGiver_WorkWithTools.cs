using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_WorkWithTools : WorkGiver
    {
        public IEnumerable<Thing> availableTools;
        public Thing closestAvailableTool;
        public string toolWorkTypeName;

        // Collection of pawns weapons before changing them to tools. Initialize with dummy element to avoid null references
        public static Dictionary<Pawn, Thing> weaponReservations = new Dictionary<Pawn, Thing>()
            {
                {new Pawn(), new Thing()}
            };

        public void WorkPenaltyCheck(Pawn pawn)
        {
            //float plantWorkSpeed_default = pawn.GetStatValue(StatDefOf.PlantWorkSpeed);
            float plantWorkSpeed_withPenalty = pawn.GetStatValue(StatDefOf.PlantWorkSpeed) / 4;
            if (pawn.equipment.Primary == null || !pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
            {
                pawn.def.SetStatBaseValue(StatDefOf.PlantWorkSpeed, plantWorkSpeed_withPenalty);
            }
            //else pawn.def.SetStatBaseValue(StatDefOf.PlantWorkSpeed, plantWorkSpeed_default);
        }

        // checks if thing has it's free stockpile cell
        public bool StorageCellExists(Pawn pawn, Thing thing)
        {
            foreach (var slotGroup in Find.SlotGroupManager.AllGroupsListInPriorityOrder)
            {
                foreach (var cell in slotGroup.CellsList.Where(cell => StoreUtility.IsValidStorageFor(cell, thing) && pawn.CanReserve(cell)))
                    if (cell != IntVec3.Invalid)
                        return true;
            }

            return false;
        }

        public void FindAvailableTools(Pawn pawn)
        {
            // choose tools by def name
            availableTools = Find.ListerThings.AllThings.FindAll(tool => tool.def.defName.Contains(toolWorkTypeName));

            // needed to avoid null references if availableTools == 0
            if (availableTools.Count() > 0)
            {
                // choose tools which are non forbidden and can be reserved
                availableTools = availableTools.Where(tool => !tool.IsForbidden(pawn.Faction) && pawn.CanReserve(tool));
            }
        }

        public bool GiveJobIfToolsAvailable(Pawn pawn)
        {
            // tools availability check for reserve and non-forbidden
            FindAvailableTools(pawn);

            if (availableTools.Count() > 0)
            {
                return true;
            }
            // no free tools
            else if (availableTools.Count() == 0)
            {
                // hands free
                if (pawn.equipment.Primary == null)
                {
                    return false;
                }
                // tool required equipped
                else if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
                {
                    return true;
                }
                // hands occupied, but it's not required tool
                else
                {
                    return false;
                }
            }
            // unexpected exception
            else
            {
                Log.Message("GiveJobIfToolsAvailable EXCEPTION");
                return false;
            }
        }

        // equip previously equipped non tool weapon
        public Job EquipReservedWeapon(Pawn pawn)
        {
            Thing weapon = weaponReservations[pawn];
            weaponReservations.Remove(pawn);
            return new Job(JobDefOf.Equip, weapon);
        }

        public Job EquipTool(Pawn pawn)
        {
            // find closest reachable tool of the specific workType name
            closestAvailableTool = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableTools, PathEndMode.Touch, TraverseParms.For(pawn), 9999);

            // job to equip nearest tool
            return new Job(JobDefOf.Equip, closestAvailableTool);
        }

        public Job ReturnTool(Pawn pawn)
        {
            // NOTE: temporary solution using default jobs, need future improvement
            ThingWithComps tool;
            // drops primary equipment (weapon) as forbidden
            pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out tool, pawn.Position);
            // making it non forbidden
            tool.SetForbidden(false);
            // vanilla haul to best stockpile job
            return HaulAIUtility.HaulToStorageJob(pawn, tool);
        }

        // for debug. Not used
        public void GetReservedWeaponsList()
        {
            foreach (KeyValuePair<Pawn, Thing> pair in weaponReservations)
            {
                Log.Message("pair =" + pair.ToString());
            }
        }
    }
}
