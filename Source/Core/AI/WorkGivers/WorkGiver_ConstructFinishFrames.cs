using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_ConstructFinishFrames : WorkGiver_WorkWithTools
    {
        public IEnumerable<Thing> availableFrames;
        public Thing closestAvailableFrame;
        //used to avoid conflict with Repair jobs cause using the same tool workType
        public static bool reserveTool = false;

        public WorkGiver_ConstructFinishFrames()
        {
            toolWorkTypeName = "Tool_Building";
        }

        public void FindAvailableFrames(Pawn pawn)
        {
            // find all unfinished frames
            availableFrames = Find.ListerThings.AllThings.FindAll(unfinishedFrame => unfinishedFrame is Frame);

            // needed to avoid null references if availableFrames == 0
            if (availableFrames.Count() > 0)
            {
                // frame belong to colony, can be built, has required materials
                availableFrames = availableFrames.Where(unfinishedFrame => unfinishedFrame.Faction == pawn.Faction && GenConstruct.CanConstruct(unfinishedFrame, pawn) && (unfinishedFrame as Frame).MaterialsNeeded().Count == 0);
            }
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // skip everything if no appropriate tools for this job
            if (!GiveJobIfToolsAvailable(pawn)) return null;

            FindAvailableFrames(pawn);

            // available frames present
            if (availableFrames.Count() > 0)
            {
                closestAvailableFrame = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableFrames, PathEndMode.Touch, TraverseParms.For(pawn), 9999);
                // forbid other workgivers to drop tool while these jobs available
                reserveTool = true;

                // hands free
                if (pawn.equipment.Primary == null)
                {
                    //reserve frame for future work
                    ReservationUtility.Reserve(pawn, closestAvailableFrame);
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
                        // finish frame job allocation
                        return new Job(JobDefOf.FinishFrame, closestAvailableFrame);
                    }
                    // tool in hands, but not appropriate
                    else if (pawn.equipment.Primary.def.defName.Contains("Tool"))
                    {
                        //reserve frame for future work
                        ReservationUtility.Reserve(pawn, closestAvailableFrame);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                    // not tool in hands
                    else
                    {
                        // reserve frame for future work
                        ReservationUtility.Reserve(pawn, closestAvailableFrame);
                        // add current weapon to reservation list to reserve it later
                        weaponReservations.Add(pawn, pawn.equipment.Primary);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                }

            }
            // no available frames
            else if (availableFrames.Count() == 0)
            {
                reserveTool = false;
                // hands occupied
                if (pawn.equipment.Primary != null)
                {
                    // appropriate tool in hands
                    if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
                    {
                        // has place to store tool and not forbidden to do it
                        if (StorageCellExists(pawn, pawn.equipment.Primary) && WorkGiver_Repair.reserveTool == false)
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
