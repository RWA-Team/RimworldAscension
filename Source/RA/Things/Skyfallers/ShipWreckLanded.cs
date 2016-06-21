using Verse;

namespace RA
{
    public class ShipWreckLanded : DropPodLanded
    {
        public override void Tick()
        {
            base.Tick();
            if (!cargo.containedThings.NullOrEmpty() && cargo.openDelay-- <= 0)
                Deploy();
        }
    }
}
