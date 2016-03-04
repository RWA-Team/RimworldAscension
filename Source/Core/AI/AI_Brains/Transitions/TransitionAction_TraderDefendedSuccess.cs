﻿using RimWorld;
using RimWorld.SquadAI;
using Verse;

namespace RA
{
    class TransitionAction_TraderDefendSuccess : TransitionAction
    {
        public string message = "TraderDefendSuccess";
        public float goodwillChange = 2.5f;

        public override void DoAction(Transition trans)
        {
            var states = trans.sources;
            foreach (var state in states)
            {
                var brainFaction = state.brain?.faction;
                if (brainFaction != null)
                {
                    brainFaction.AffectGoodwillWith(Faction.OfColony, goodwillChange);
                    Messages.Message(message.Translate(brainFaction.name), MessageSound.Silent);
                }
            }
        }

    }
}
