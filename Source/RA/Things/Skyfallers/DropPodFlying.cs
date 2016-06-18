using RimWorld;
using Verse;

namespace RA
{
    public class DropPodFlying : SkyfallerFlying
    {
        public DropPodInfo cargo;

        public override void SpawnSetup()
        {
            // Do base setup
            base.SpawnSetup();

            rotSpeed = 7;

            impactResultThing = (DropPodLanded)ThingMaker.MakeThing(ThingDef.Named("DropPodLanded"));
            (impactResultThing as DropPodLanded).cargo = cargo;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.LookDeep(ref cargo, "cargo");
        }
    }
}
