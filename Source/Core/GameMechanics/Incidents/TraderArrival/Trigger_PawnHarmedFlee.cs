using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;
using RimWorld.SquadAI;

namespace RimworldAscension
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
            else
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
            return (signal.condition == PawnLostCondition.TakenPrisoner ? true : signal.condition == PawnLostCondition.IncappedOrKilled);
        }

        public bool ActivateOnPawnGroup(Brain brain, TriggerSignal signal)
        {
            if (signal.type != TriggerSignalType.PawnLost)
            {
                return false;
            }
            return (float)brain.numPawnsLostViolently >= (float)brain.numPawnsEverGained * this.fraction;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<float>(ref this.fraction, "fraction", 0.5f, false);
        }
    }
}
