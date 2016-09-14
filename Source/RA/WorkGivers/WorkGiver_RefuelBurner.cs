using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_RefuelBurner : WorkGiver
    {
        IEnumerable<Thing> targetBurners;

        public override bool ShouldSkip(Pawn pawn)
        {
            targetBurners = UnreservedBurnersToRefuel(pawn);
            return !targetBurners.Any();
        }

        public override Job NonScanJob(Pawn pawn)
        {
            var closestBurner = ClosestBurnerToRefuel(pawn);

            if (closestBurner != null)
            {
                var availableFuel = from fuel in Find.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                    where fuel.GetStatValue(StatDef.Named("BurnDurationHours")) > 0f
                          && closestBurner.filterFuelCurrent.Allows(fuel)
                          && HaulAIUtility.PawnCanAutomaticallyHaul(pawn, fuel)
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

                    if (closestFuel != null)
                    {
                        return new Job(DefDatabase<JobDef>.GetNamed("RefuelBurner"), closestFuel, closestBurner.parent)
                        {
                            maxNumToCarry = numToCarry
                        };
                    }
                }
            }
            return null;
        }
        
        public IEnumerable<Thing> UnreservedBurnersToRefuel(Pawn pawn)
        {
            return Find.ListerThings.AllThings.Where(thing => thing.TryGetComp<CompFueled>()!=null && thing.TryGetComp<CompFueled>().RequireMoreFuel && pawn.CanReserve(thing));
        }

        public CompFueled ClosestBurnerToRefuel(Pawn pawn)
        {
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, targetBurners, PathEndMode.Touch, TraverseParms.For(pawn, pawn.NormalMaxDanger())).TryGetComp<CompFueled>();
        }
    }
}
