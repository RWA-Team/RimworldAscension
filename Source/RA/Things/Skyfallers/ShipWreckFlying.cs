using Verse;

namespace RA
{
    public class ShipWreckFlying : DropPodFlying
    {
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            ticksToImpact = Rand.RangeInclusive(300, 500);
            impactResultThing = (DropPodLanded)ThingMaker.MakeThing(ThingDef.Named("ShipWreckLanded"));
            (impactResultThing as DropPodLanded).cargo = cargo;

            rotSpeed = 3;
        }
    }
}
