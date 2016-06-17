using Verse;

namespace RA
{
    public class DebrisFlying : SkyfallerFlying
    {
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            rotSpeed = 20;

            impactResultThing = ThingMaker.MakeThing(ThingDef.Named("ChunkSlagSteel"));
        }
    }
}
