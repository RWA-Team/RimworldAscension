using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{

    /// <summary>
    /// The JobDriver to install the weapon into the turret
    /// </summary>
    /// <author>Haplo</author>
    /// <permission>Please check the provided license info for granted permissions.</permission>
    public class JobDriver_GoTrading : JobDriver
    {
        public ICommunicable tradeCompany;
        public const TargetIndex targetInd = TargetIndex.A;

        public override string GetReport()
        {
            string repString;

            repString = !LanguageDatabase.activeLanguage.TryGetTextFromKey("ReportTrading", out repString) ? base.GetReport() : "ReportTrading".Translate();

            if (pawn != null && !pawn.Destroyed && pawn.SpawnedInWorld && pawn.CurJob != null)
            {
                if (!pawn.CurJob.def.reportString.NullOrEmpty())
                    repString = pawn.jobs.curJob.def.reportString;
            }

            return repString;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Set fail conditions
            this.FailOnDestroyed(targetInd);
            this.FailOnBurningImmobile(targetInd);
            //Note we only fail on forbidden if the target doesn't start that way
            //This helps haul-aside jobs on forbidden items
            if (!TargetThingA.IsForbidden(pawn.Faction))
                this.FailOnForbidden(targetInd);

            //Reserve thing to be used
            var reserveTargetA = Toils_Reserve.Reserve(targetInd);
            yield return reserveTargetA;

            // Goto object
            Toil toilGoto = null;
            toilGoto = Toils_Goto.GotoThing(targetInd, PathEndMode.InteractionCell)
                .FailOn(() =>
                {
                    //Note we don't fail on losing hauling designation
                    //Because that's a special case anyway

                    //While hauling to cell storage, ensure storage dest is still valid
                    var actor = toilGoto.actor;
                    var curJob = actor.jobs.curJob;

                    var targetThing = curJob.GetTarget(targetInd).Thing;

                    if (targetThing == null || targetThing.Destroyed || targetThing.IsBurning() || targetThing.IsForbidden(pawn.Faction))
                        return true;

                    return false;
                });
            yield return toilGoto;


            // start trading with target
            tradeCompany = pawn.jobs.curJob.commTarget;
            yield return Toils_OpenTradingWindow(tradeCompany);

        }

        public Toil Toils_OpenTradingWindow(ICommunicable tradeCompany)
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                toil.actor.pather.StopDead();
                tradeCompany.TryOpenComms(toil.actor);
            };

            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 60;

            return toil;
        }
    }
}
