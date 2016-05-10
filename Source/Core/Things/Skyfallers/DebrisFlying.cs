using Verse;

namespace RA
{
    public class DebrisFlying : SkyfallerFlying
    {
        public override void SpawnSetup()
        {
            // Do base setup
            base.SpawnSetup();
            
            impactResultThing = ThingMaker.MakeThing(ThingDef.Named("ChunkSlagSteel"));
        }
    }
}
