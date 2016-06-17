using RimWorld;
using Verse;

namespace RA
{
    // special spawns for skyfaller impact explosion (otherwise things are damaged by eplosion even if spawned after that, but without delay)
    public class RA_Explosion : Explosion
    {
        public void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 cell)
        {
            if (thingDef == null)
            {
                return;
            }
            if (thingDef.thingClass == typeof (LiquidFuel))
            {
                var liquidFuel = (LiquidFuel) Find.ThingGrid.ThingAt(cell, thingDef);
                if (liquidFuel != null)
                {
                    liquidFuel.Refill();
                    return;
                }
            }

            // special skyfaller spawning
            if (thingDef == ThingDef.Named("CobbleSlate"))
            {
                var impactResultThing = ThingMaker.MakeThing(ThingDef.Named("CobbleSlate"));
                impactResultThing.stackCount = Rand.RangeInclusive(1, 10);
                GenPlace.TryPlaceThing(impactResultThing, cell, ThingPlaceMode.Near);
                return;
            }

            GenSpawn.Spawn(thingDef, cell);
        }
    }
}