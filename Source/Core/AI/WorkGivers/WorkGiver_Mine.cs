using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Mine : WorkGiver_WorkWithTools
    {
        public IEnumerable<Thing> availableVeins;
        public Thing closestAvailableVein;

        public WorkGiver_Mine()
        {
            toolWorkTypeName = "Tool_Mining";
        }

        public void FindAvailableVeins(Pawn pawn)
        {
            // find all veins designated to be mined
            availableVeins = Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Mine).Select(designation => MineUtility.MineableInCell(designation.target.Cell));

            // needed to avoid null references if availableVeins == 0
            if (availableVeins.Count() > 0)
            {
                // could be reserved and reached
                availableVeins = availableVeins.Where(vein => pawn.CanReserveAndReach(vein, PathEndMode.Touch, pawn.NormalMaxDanger()));
            }
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // skip everything if no appropriate tools for this job
            if (!GiveJobIfToolsAvailable(pawn)) return null;

            FindAvailableVeins(pawn);

            // available veins present
            if (availableVeins.Count() > 0)
            {
                closestAvailableVein = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableVeins, PathEndMode.Touch, TraverseParms.For(pawn), 9999);

                // hands free
                if (pawn.equipment.Primary == null)
                {
                    //reserve vein for future work
                    ReservationUtility.Reserve(pawn, closestAvailableVein);
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

                        // new mining scheme without hauling rubble
                        return new Job(JobDefOf.Mine, closestAvailableVein, 1500, true);
                    }
                    // tool in hands, but not appropriate
                    else if (pawn.equipment.Primary.def.defName.Contains("Tool"))
                    {
                        //reserve vein for future work
                        ReservationUtility.Reserve(pawn, closestAvailableVein);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                    // not tool in hands
                    else
                    {
                        // reserve vein for future work
                        ReservationUtility.Reserve(pawn, closestAvailableVein);
                        // add current weapon to reservation list to reserve it later
                        weaponReservations.Add(pawn, pawn.equipment.Primary);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                }

            }
            // no available veins
            else if (availableVeins.Count() == 0)
            {
                // hands occupied
                if (pawn.equipment.Primary != null)
                {
                    // appropriate tool in hands
                    if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
                    {
                        // has place to store tool
                        if (StorageCellExists(pawn, pawn.equipment.Primary))
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
