using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_MakeSignalSmoke : JobDriver
    {
        public const TargetIndex CampfireInd = TargetIndex.A;

        int ticks = GenDate.SecondsToTicks(1) * 5;
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CampfireInd);
            this.FailOnBurningImmobile(CampfireInd);
            
            yield return Toils_Goto.GotoThing(CampfireInd, PathEndMode.InteractionCell);
            yield return Toils_Reserve.Reserve(CampfireInd);
            yield return WaitUntilBurnerReady();
            yield return DoSignalSmoke(ticks);
        }

        // ignites burner to start consume fuel, if needed
        public Toil WaitUntilBurnerReady()
        {
            var burner = CurJob.GetTarget(TargetIndex.A).Thing as RA_Building_WorkTable;

            var toil = new Toil
            {
                initAction = () =>
                {
                    if (burner == null)
                    {
                        return;
                    }
                    pawn.pather.StopDead();
                },
                tickAction = () =>
                {
                    //if (burner.internalTemp > burner.compFueled.Properties.operatingTemp)
                    //    ReadyForNextToil();
                }
            };
            // fails if no more heat generation and temperature is no enough
            //toil.FailOn(() => burner.currentFuelBurnDuration == 0 && !burner.UsableNow);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }

        // ignites burner to start consume fuel, if needed
        public Toil DoSignalSmoke(int ticks)
        {
            var campfire = CurJob.GetTarget(TargetIndex.A).Thing as Campfire;

            var toil = new Toil
            {
                tickAction = () =>
                {
                    // NOTE: needed?
                    ticks--;
                }
            };
            // fails if no more heat generation and temperature is no enough
            //toil.FailOn(() => campfire.currentFuelBurnDuration == 0 && !campfire.UsableNow);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = ticks;
            //toil.AddFinishAction(campfire.CallForMilitaryAid);
            return toil;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue(ref ticks, "ticks");
        }
    }
}