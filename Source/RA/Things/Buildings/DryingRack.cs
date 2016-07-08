using RimWorld;
using Verse;

namespace RA
{
    public class DryingRack : Building_Storage
    {
        public Thing dryingThing;
        public int dryingProgress;

        public override void TickLong()
        {
            if (dryingThing != null) CheckIfDried();
        }

        public void CheckIfDried()
        {
            if (dryingProgress-- <= 0)
            {
                var driedMeat = ThingMaker.MakeThing(ThingDef.Named(dryingThing.def.defName + "Dried"));
                driedMeat.stackCount = dryingThing.stackCount;
                dryingThing.Destroy();
                GenPlace.TryPlaceThing(driedMeat, Position, ThingPlaceMode.Direct);
            }
        }

        public override void Notify_ReceivedThing(Thing receivedThing)
        {
            if (dryingThing != receivedThing && receivedThing.def.defName.Contains("Meat") && !receivedThing.def.defName.Contains("Dried"))
            {
                dryingThing = receivedThing;
                dryingProgress = GenDate.TicksPerDay / GenTicks.TickLongInterval;
            }
        }

        public override void Notify_LostThing(Thing lostThing)
        {
            dryingThing = null;
        }
    }
}
