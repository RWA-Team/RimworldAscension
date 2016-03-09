using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.SquadAI;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public class RA_Brain: Brain
	{
        //public State curState;
        //public StateGraph graph;
        //public int ticksInState;
        //private List<Thing> cachedAttackTargets = new List<Thing>();

        List<State> currentStates = new List<State>();
        List<int> currentTicks = new List<int>();
        List<int> triggeredStates = new List<int>();

        #region SEEMS OK

        public new void ExposeData()
		{
			Scribe_Values.LookValue(ref loadID, "loadID", 0, false);
			Scribe_References.LookReference(ref faction, "faction");
			Scribe_References.LookReference(ref curState, "curState");
			Scribe_Values.LookValue(ref ticksInState, "ticksInState", 0, false);
			Scribe_Values.LookValue(ref numPawnsEverGained, "numPawnsEverGained", 0, false);
			Scribe_Values.LookValue(ref numPawnsLostViolently, "numPawnsLostViolently", 0, false);
			Scribe_Collections.LookList(ref ownedPawns, "ownedPawns", LookMode.MapReference);
			Scribe_Deep.LookDeep(ref graph, "graph");
			Scribe_Deep.LookDeep(ref avoidGrid, "avoidGrid");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < graph.states.Count; i++)
				{
					graph.states[i].brain = this;
				}
			}
		}

		public new void Cleanup()
		{
		    foreach (var state in currentStates)
		    {
                state.Cleanup();
            }
        }

        public new void AddPawn(Pawn pawn)
        {
            if (ownedPawns.Contains(pawn))
            {
                Log.Error(string.Concat("SquadBrain for ", faction, " tried to add ", pawn, " whom it already controls."));
                return;
            }

            ownedPawns.Add(pawn);
            numPawnsEverGained++;

            currentStates.Add(curState);
            currentTicks.Add(0);
            foreach (var state in currentStates)
            {
                state.UpdateAllDuties();
            }
        }

        public new void SquadBrainTick()
        {
            for (int i = 0; i < ownedPawns.Count; i++)
            {
                currentStates[i].StateTick();
                currentTicks[i]++;
            }
            CheckTransitionOnSignal(TriggerSignal.ForTick);
        }

        private void RemovePawn(Pawn pawn)
        {
            var index = ownedPawns.IndexOf(pawn);
            ownedPawns.RemoveAt(index);
            currentTicks.RemoveAt(index);
            currentStates.RemoveAt(index);

            if (ownedPawns.Count == 0)
            {
                Find.SquadBrainManager.RemoveSquadBrain(this);
            }
        }

        #endregion

        public new void GotoState(State newState)
		{
			if (curState != null)
			{
				curState.Cleanup();
			}
            Cleanup();

            curState = newState;

			ticksInState = 0;
			if (curState.brain != this)
			{
				Log.Error("curState brain is " + curState.brain);
				curState.brain = this;
			}
			curState.Init();
			for (int i = 0; i < graph.transitions.Count; i++)
			{
				if (graph.transitions[i].sources.Contains(curState))
				{
					graph.transitions[i].SourceStateBecameActive();
				}
			}
			curState.UpdateAllDuties();
			foreach (Pawn current in ownedPawns.ToList())
			{
				if (current.jobs.curJob != null)
				{
					current.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}

        // checks if incoming trigger signal makes any of current states to transit to another
        private CheckTransitionOnSignal(TriggerSignal signal)
        {
            triggeredStates.Clear();
            // TODO: do i need that check?
            if (!ownedPawns.NullOrEmpty())
            {
                for (var i = 0; i < ownedPawns.Count; i++)
                {
                    foreach (Transition transition in graph.transitions)
                    {
                        if (transition.sources.Contains(currentStates[i]) &&
                            transition.CheckSignal(this, signal))
                        {
                            triggeredStates.Add(i);
                            // TODO: do i need that?
                            break;
                        }
                    }
                }
            }
        }
	}
}
