using Verse;

namespace RA
{
    public class WorkTableFueled : RA_Building_WorkTable
    {
        // defining required comps here for convinience
        public CompFueled compFueled;

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            // required comps initialize
            compFueled = this.TryGetComp<CompFueled>();
        }

        // used to determine if using bills is possible
        public override bool UsableNow => compFueled == null
                                          || compFueled.internalTemp > compFueled.Properties.operatingTemp
                                          || compFueled.fuelContainer.Count > 0 && compFueled.fuelContainer[0].stackCount > 0;
    }
}