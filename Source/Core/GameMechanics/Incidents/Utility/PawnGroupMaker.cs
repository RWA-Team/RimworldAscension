using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;


namespace RimWorld
{
    public class PawnGroupMaker
    {
        //Config
        public float commonality = 100;
        public List<PawnGenOption> options = new List<PawnGenOption>();
        private List<RaidStrategyDef> disallowedStrategies = null;

        //Working vars
        [Unsaved]
        private static List<Pawn> makingPawns = new List<Pawn>();

        //Constants
        private const string MeleeWeaponTag = "Melee";

        //Properties
        public static List<Pawn> MakingPawns { get { return makingPawns; } }
        public float MinPointsToGenerateAnything { get { return options.Min(g => g.Cost); } }



        public bool CanGenerateFrom(IncidentParms parms)
        {
            if (disallowedStrategies != null && disallowedStrategies.Contains(parms.raidStrategy))
                return false;

            return ChoosePawnGenOptions(parms).Any();
        }

        public IEnumerable<Pawn> GenerateArrivingPawns(IncidentParms parms, bool errorOnZeroResults = true)
        {
            if (!CanGenerateFrom(parms))
            {
                if (errorOnZeroResults)
                    Log.Error("Cannot generate arriving pawns for " + parms.faction + " with " + parms.points + ". Defaulting to a single random cheap group.");

                yield break;
            }

            //Spawn the pawns from the chosen groups
            bool forceIncapDone = false;
            foreach (var g in ChoosePawnGenOptions(parms))
            {
                Pawn p;
                do
                {
                    p = PawnGenerator.GeneratePawn(g.kind, parms.faction);
                }
                while (p.story != null && p.story.WorkTagIsDisabled(WorkTags.Violent));

                //Force incap one raider
                if (parms.raidForceOneIncap && !forceIncapDone)
                {
                    p.health.forceIncap = true;
                    p.mindState.canFleeIndividual = false;
                    forceIncapDone = true;
                }

                makingPawns.Add(p);

                yield return p;
            }

            makingPawns.Clear();
        }

        private IEnumerable<PawnGenOption> ChoosePawnGenOptions(IncidentParms parms)
        {
            //Spend the points group by group
            float initialPoints = parms.points;
            float pointsLeft = parms.points;
            List<PawnGenOption> allowedGroups = new List<PawnGenOption>();
            List<PawnGenOption> chosenGroups = new List<PawnGenOption>();
            bool leaderChosen = false;

            while (true)
            {
                allowedGroups.Clear();

                for (int i = 0; i < options.Count; i++)
                {
                    var g = options[i];

                    if (g.Cost > pointsLeft)
                        continue;

                    if (g.Cost > MaxAllowedPawnGroupCost(parms.faction, parms.points, parms.raidStrategy))
                        continue;

                    if (parms.generateFightersOnly && !g.kind.isFighter)
                        continue;

                    if (parms.generateMeleeOnly && g.kind.weaponTags.Any(tag => tag != MeleeWeaponTag))
                        continue;

                    if (parms.raidStrategy != null && !parms.raidStrategy.Worker.AllowChoosePawnGenOption(g, chosenGroups))
                        continue;

                    allowedGroups.Add(g);
                }

                if (leaderChosen)
                    allowedGroups.RemoveAll(group => group.kind.factionLeader);

                if (allowedGroups.Count == 0)
                    break;

                //At higher points counts, we suppress the pawn count by biasing up the weight
                // of groups with a high points cost per pawn
                float desireToSuppressCount = Mathf.InverseLerp(800, 1600, initialPoints);
                desireToSuppressCount = Mathf.Clamp(desireToSuppressCount, 0, 0.5f);
                Func<PawnGenOption, float> adjustedSelectionWeight = gr =>
                {
                    float weight = gr.selectionWeight;

                    if (desireToSuppressCount > 0)
                    {
                        float weightAdjusted = weight * gr.Cost;
                        weight = Mathf.Lerp(weight, weightAdjusted, desireToSuppressCount);
                    }

                    return weight;
                };

                PawnGenOption newlyChosenEntry = allowedGroups.RandomElementByWeight(adjustedSelectionWeight);
                if (newlyChosenEntry.kind.factionLeader)
                    leaderChosen = true;

                chosenGroups.Add(newlyChosenEntry);
                pointsLeft -= newlyChosenEntry.Cost;
            }

            return chosenGroups;
        }

        /// <summary>
        /// Returns the maximum cost of a pawn group we can select given a set of total points and a raid style.
        /// Note that the cost changes depending on raid style because siege requires that we have 4 pawns, so we can't
        /// spend too much on each one.
        /// </summary>
        private static float MaxAllowedPawnGroupCost(Faction faction, float totalPoints, RaidStrategyDef raidStrategy)
        {
            float maxGroupCost = Mathf.Max(totalPoints * 0.5f, 50);

            if (raidStrategy != null)
            {
                //Ensure we have enough pawns to satisfy minimum pawn count
                maxGroupCost = Mathf.Min(maxGroupCost, totalPoints / raidStrategy.minPawns);
            }

            //Ensure we can at least afford the cheapest group of the faction
            maxGroupCost = Mathf.Max(maxGroupCost, faction.def.MinPointsToGeneratePawnGroup() * 1.2f);

            //Todo ensure we have the points to get the pawn type we require


            return maxGroupCost;
        }

    }
}