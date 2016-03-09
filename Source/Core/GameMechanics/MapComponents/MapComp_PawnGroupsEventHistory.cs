using System.Collections.Generic;
using RimWorld;
using RimWorld.SquadAI;
using Verse;

namespace RA
{
    public class MapComp_PawnGroupsEventHistory : MapComponent
    {
        public const float MaxPenaltyFor_HarmPawn = -30f;
        public const float MaxPenaltyFor_KillPawn = -75f;

        public const float PenaltyFor_HarmPawn = -10f;
        public const float PenaltyFor_KillPawn = -25f;

        public Dictionary<Brain, Dictionary<string, int>> eventsHistory = new Dictionary<Brain, Dictionary<string, int>>();
        public Dictionary<string, int> counters = new Dictionary<string, int>();

        public void Notify_GoodwillEffectOccured(Brain brain, string eventName)
        {
            if (eventsHistory.ContainsKey(brain))
            {
                if (eventsHistory[brain].ContainsKey(eventName))
                {
                    eventsHistory[brain][eventName]++;
                }
                else
                    eventsHistory[brain].Add(eventName, 1);
            }
        }

        public void Notify_GroupDisappeared(Brain brain)
        {
            if (eventsHistory.ContainsKey(brain))
            {
                ApplyEvents(brain);
                eventsHistory.Remove(brain);
            }
        }

        // only the hardest crime applies at the same time
        public void ApplyEvents(Brain brain)
        {
            foreach (var counter in eventsHistory[brain])
            {
                float penalty;
                switch (counter.Key)
                {
                    case "killPawn":
                        penalty = PenaltyFor_KillPawn*counter.Value < MaxPenaltyFor_KillPawn
                            ? PenaltyFor_KillPawn*counter.Value
                            : MaxPenaltyFor_KillPawn;
                        brain.faction.AffectGoodwillWith(Faction.OfColony, penalty);
                        return;
                    case "harmPawn":
                        penalty = PenaltyFor_HarmPawn * counter.Value < MaxPenaltyFor_HarmPawn
                            ? PenaltyFor_KillPawn * counter.Value
                            : MaxPenaltyFor_KillPawn;
                        brain.faction.AffectGoodwillWith(Faction.OfColony, penalty);
                        return;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.LookDictionary(ref eventsHistory, "eventsHistory", LookMode.Deep, LookMode.Deep);
        }
    }
}
