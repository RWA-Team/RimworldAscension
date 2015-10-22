using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;
using RimWorld.SquadAI;

namespace RA
{
    class Trigger_PawnHarmed_NotIfPawnHarmedFlee : Trigger
    {
        Trigger_PawnHarmedFlee fleeTrigger;

        public Trigger_PawnHarmed_NotIfPawnHarmedFlee()
        {
            fleeTrigger = new Trigger_PawnHarmedFlee();
        }
        public Trigger_PawnHarmed_NotIfPawnHarmedFlee(float fraction)
        {
            fleeTrigger = new Trigger_PawnHarmedFlee(fraction);
        }


        public override bool ActivateOn(Brain brain, TriggerSignal signal)
        {
            // do nothing, if flee trigger is fired
            if (fleeTrigger.ActivateOn(brain, signal))
                return false;

            if (signal.type == TriggerSignalType.PawnDamaged)
                return signal.dinfo.Def.externalViolence;

            if (signal.type != TriggerSignalType.PawnLost)
                return false;

            return (signal.condition == PawnLostCondition.TakenPrisoner ? true : signal.condition == PawnLostCondition.IncappedOrKilled);
        }

    }
}
