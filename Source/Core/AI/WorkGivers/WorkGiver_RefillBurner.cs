using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_RefillBurner : WorkGiver
    {
        IEnumerable<Thing> targetBurners;

        public override bool ShouldSkip(Pawn pawn)
        {
            targetBurners = UnreservedBurnersToRefill(pawn);
            return !targetBurners.Any();
        }

        public override Job NonScanJob(Pawn pawn)
        {
            var closestBurner = ClosestBurnerToRefill(pawn);

            if (closestBurner != null)
            {
                var availableFuel = from fuel in Find.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                                                   where fuel.GetStatValue(StatDef.Named("BurnDurationHours")) > 0f && HaulAIUtility.PawnCanAutomaticallyHaul(pawn, fuel) && closestBurner.filterFuelCurrent.Allows(fuel)
                                                   select fuel;

                if (availableFuel.Any())
                {
                    Thing closestFuel;
                    int numToCarry;
                    // if burner is empty, any closest fuel type will do
                    if (closestBurner.fuelContainer.Count == 0)
                    {
                        closestFuel = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableFuel, PathEndMode.ClosestTouch, TraverseParms.For(pawn, pawn.NormalMaxDanger()));
                        numToCarry = closestFuel.def.stackLimit;
                    }
                    // else, only closest fuel type of the same def will do
                    else
                    {
                        closestFuel = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableFuel, PathEndMode.ClosestTouch, TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999, fuel => fuel.def == closestBurner.fuelContainer[0].def);
                        numToCarry = closestBurner.fuelContainer[0].def.stackLimit - closestBurner.fuelContainer[0].stackCount;
                    }

                    return new Job(DefDatabase<JobDef>.GetNamed("RefillBurner"), closestFuel, closestBurner)
                    {
                        maxNumToCarry = numToCarry
                    };
                }
            }
            return null;
        }
        
        public IEnumerable<Thing> UnreservedBurnersToRefill(Pawn pawn)
        {
            return Find.ListerBuildings.AllBuildingsColonistOfClass<WorkTable_Fueled>().Where(burner => burner.RequireMoreFuel() && pawn.CanReserve(burner)).Cast<Thing>();
        }

        public WorkTable_Fueled ClosestBurnerToRefill(Pawn pawn)
        {
            return (WorkTable_Fueled)GenClosest.ClosestThing_Global_Reachable(pawn.Position, targetBurners, PathEndMode.Touch, TraverseParms.For(pawn, pawn.NormalMaxDanger()));
        }
    }
}
