using RimWorld.SquadAI;
using Verse;
//using RimWorld.Planet;

namespace RA
{
    class Trigger_PawnHarmedFlee : Trigger
    {
        public float fraction = 0.5f; // percentage of pawns to be lost within the group until flee

        public Trigger_PawnHarmedFlee() {}
        public Trigger_PawnHarmedFlee(float fraction)
        {
            this.fraction = fraction;
        }

        public override bool ActivateOn(Brain brain, TriggerSignal signal)
        {
            if (brain.numPawnsEverGained > 1)
                return ActivateOnPawnGroup(brain, signal);
            return ActivateOnPawnSingle(brain, signal);
        }

        public bool ActivateOnPawnSingle(Brain brain, TriggerSignal signal)
        {
            if (signal.type == TriggerSignalType.PawnDamaged)
            {
                return signal.dinfo.Def.externalViolence;
            }
            if (signal.type != TriggerSignalType.PawnLost)
            {
                return false;
            }
            return signal.condition == PawnLostCondition.TakenPrisoner || signal.condition == PawnLostCondition.IncappedOrKilled;
        }

        public bool ActivateOnPawnGroup(Brain brain, TriggerSignal signal)
        {
            if (signal.type != TriggerSignalType.PawnLost)
            {
                return false;
            }
            return brain.numPawnsLostViolently >= brain.numPawnsEverGained * fraction;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue(ref fraction, "fraction", 0.5f);
        }
    }
}
