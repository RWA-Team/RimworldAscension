using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Repair : WorkGiver_WorkWithTools
    {
        public IEnumerable<Thing> availableRepairables;
        public Thing closestAvailableRepairable;
        //used to avoid conflict with Construction jobs cause using the same tool workType
        public static bool reserveTool = false;

        public WorkGiver_Repair()
        {
            toolWorkTypeName = "Tool_Building";
        }

        public void FindAvailableRepairables(Pawn pawn)
        {
            // find all available repairables
            availableRepairables = ListerBuildingsRepairable.RepairableBuildings(pawn.Faction);

            // needed to avoid null references if availableRepairables == 0
            if (availableRepairables.Count() > 0)
            {
                // repairable is in home area, can e reserved, not being decontructed, not burning
                availableRepairables = availableRepairables.Where(Repairable => pawn.Faction == Faction.OfColony && Find.AreaHome[Repairable.Position] && pawn.CanReserve(Repairable) && Repairable.def.useHitPoints && Repairable.HitPoints < Repairable.MaxHitPoints && Find.DesignationManager.DesignationOn(Repairable, DesignationDefOf.Deconstruct) == null && !Repairable.IsBurning());
            }
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // skip everything if no appropriate tools for this job
            if (!GiveJobIfToolsAvailable(pawn)) return null;

            FindAvailableRepairables(pawn);

            // available repairables present
            if (availableRepairables.Count() > 0)
            {
                closestAvailableRepairable = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableRepairables, PathEndMode.Touch, TraverseParms.For(pawn), 9999);
                // forbid other workgivers to drop tool while these jobs available
                reserveTool = true;

                // hands free
                if (pawn.equipment.Primary == null)
                {
                    //reserve repairable for future work
                    ReservationUtility.Reserve(pawn, closestAvailableRepairable);
                    return EquipTool(pawn);
                }
                // hands occupied
                else
                {
                    // appropriate tool in hands
                    if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
                    {
                        // if reservation of previous weapon was not made, make one
                        if (weaponReservations.ContainsKey(pawn) && pawn.CanReserve(weaponReservations[pawn]))
                        {
                            ReservationUtility.Reserve(pawn, weaponReservations[pawn]);
                        }
                        // finish repairable job allocation
                        return new Job(JobDefOf.Repair, closestAvailableRepairable);
                    }
                    // tool in hands, but not appropriate
                    else if (pawn.equipment.Primary.def.defName.Contains("Tool"))
                    {
                        //reserve repairable for future work
                        ReservationUtility.Reserve(pawn, closestAvailableRepairable);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                    // not tool in hands
                    else
                    {
                        // reserve repairable for future work
                        ReservationUtility.Reserve(pawn, closestAvailableRepairable);
                        // add current weapon to reservation list to reserve it later
                        weaponReservations.Add(pawn, pawn.equipment.Primary);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                }

            }
            // no available repairables
            else if (availableRepairables.Count() == 0)
            {
                reserveTool = false;
                // hands occupied
                if (pawn.equipment.Primary != null)
                {
                    // appropriate tool in hands
                    if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
                    {
                        // has place to store tool and not forbidden to do it
                        if (StorageCellExists(pawn, pawn.equipment.Primary) && WorkGiver_ConstructFinishFrames.reserveTool == false)
                        {
                            // store it
                            return ReturnTool(pawn);
                        }
                        // no storage, but has reserved weapon
                        if (weaponReservations.ContainsKey(pawn))
                        {
                            return EquipReservedWeapon(pawn);
                        }
                        // no storage, no reserve, keep the tool
                        else
                        {
                            return null;
                        }
                    }
                    // something else in hands, skip job
                    else
                    {
                        return null;
                    }
                }
                // hands free
                else
                {
                    // has reserved weapon
                    if (weaponReservations.ContainsKey(pawn))
                    {
                        return EquipReservedWeapon(pawn);
                    }
                    // skip job
                    else
                    {
                        return null;
                    }
                }
            }

            Log.Message("EXCEPTIONAL JOB SKIP");
            return null;
        }
    }
}
