using Verse;

namespace RA
{
    public class ShipWreckLanded : DropPodLanded
    {
        public bool deployed; // whether skyfaller has opened yet

        public override void Tick()
        {
            base.Tick();

            if (!deployed)
                if (cargo.openDelay-- <= 0)
                {
                    SpawnCargo();
                    deployed = true;
                }
        }

        public override void ExposeData()
        {
            // Base data to save
            base.ExposeData();

            Scribe_Values.LookValue(ref deployed, "deployed");
        }
    }
}
