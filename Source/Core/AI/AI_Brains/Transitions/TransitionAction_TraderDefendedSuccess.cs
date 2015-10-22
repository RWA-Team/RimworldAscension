using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;


namespace RA
{
    class TransitionAction_TraderDefendSuccess : TransitionAction
    {
        public string message = "TraderDefendSuccess";
        public float goodwillChange = 2.5f;

        public override void DoAction(Transition trans)
        {
            List<State> states = trans.sources;
            foreach (State state in states)
            {
                if (state.brain != null)
                {
                    List<Pawn> brainPawns = state.brain.ownedPawns;
                    Faction brainFaction = state.brain.faction;
                    if (brainFaction != null)
                    {
                        brainFaction.AffectGoodwillWith(Faction.OfColony, goodwillChange);
                        Messages.Message(message.Translate(new object[] { brainFaction.name }), MessageSound.Silent);
                    }
                }
            }
        }

    }
}
