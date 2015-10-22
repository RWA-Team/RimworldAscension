using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_PlantsCut : WorkGiver_WorkWithTools
    {
        public IEnumerable<Thing> availablePlants;
        public Thing closestAvailablePlant;

        public WorkGiver_PlantsCut()
        {
            toolWorkTypeName = "Tool_Woodcutting";
        }

        public void FindAvailablePlants(Pawn pawn)
        {
            // find all plants designated to be cut of harvest
            availablePlants = Find.DesignationManager.allDesignations.FindAll(designation => designation.def == DesignationDefOf.CutPlant || designation.def == DesignationDefOf.HarvestPlant).Select(designation => designation.target.Thing);

            // needed to avoid null references if availablePlants == 0
            if (availablePlants.Count() > 0)
            {
                // could be reserved and not on fire among them
                availablePlants = availablePlants.Where(plant => pawn.CanReserve(plant) && !plant.IsBurning());
            }
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // skip everything if no appropriate tools for this job
            if (!GiveJobIfToolsAvailable(pawn)) return null;

            FindAvailablePlants(pawn);

            // available plants present
            if (availablePlants.Count() > 0)
            {
                closestAvailablePlant = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availablePlants, PathEndMode.Touch, TraverseParms.For(pawn), 9999);

                // hands free
                if (pawn.equipment.Primary == null)
                {
                    //reserve plant for future work
                    ReservationUtility.Reserve(pawn, closestAvailablePlant);
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
                        // cut or harvest plant job allocation
                        foreach (Designation current in Find.DesignationManager.AllDesignationsOn(closestAvailablePlant))
                        {
                            if (current.def == DesignationDefOf.HarvestPlant)
                            {
                                if (current.def == DesignationDefOf.HarvestPlant && !((Plant)closestAvailablePlant).HarvestableNow)
                                {
                                    return null;
                                }
                                return new Job(JobDefOf.Harvest, closestAvailablePlant);
                            }
                            else if (current.def == DesignationDefOf.CutPlant)
                            {
                                return new Job(JobDefOf.CutPlant, closestAvailablePlant);
                            }
                        }
                    }
                    // tool in hands, but not appropriate
                    else if (pawn.equipment.Primary.def.defName.Contains("Tool"))
                    {
                        //reserve plant for future work
                        ReservationUtility.Reserve(pawn, closestAvailablePlant);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                    // not tool in hands
                    else
                    {
                        // reserve plant for future work
                        ReservationUtility.Reserve(pawn, closestAvailablePlant);
                        // add current weapon to reservation list to reserve it later
                        weaponReservations.Add(pawn, pawn.equipment.Primary);
                        // equip appropriate tool
                        return EquipTool(pawn);
                    }
                }

            }
            // no available plants
            else if (availablePlants.Count() == 0)
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
