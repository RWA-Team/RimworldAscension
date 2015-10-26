using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RA
{
    public class Genstep_ColonistsSkullywag : Genstep
    {
        public override void Generate()
        {
            // Loop over starting colonists
            foreach (Pawn current in MapInitData.colonists)
            {
                // Set their faction to ofColony
                current.SetFactionDirect(Faction.OfColony);
                // Setup our pawn with things he needs
                PawnUtility.AddAndRemoveComponentsAsAppropriate(current);
                // Add a thought
                current.needs.mood.thoughts.TryGainThought(ThoughtDefOf.NewColonyOptimism);
            }
            // Setup initial work settings
            Genstep_ColonistsSkullywag.CreateInitialWorkSettings();
            // Add a debug option
            bool startedDirectInEditor = MapInitData.StartedDirectInEditor;
            // Setup a new list
            List<List<Thing>> list = new List<List<Thing>>();
            // Loop over starting colonists
            foreach (Pawn current2 in MapInitData.colonists)
            {
                // Randomly give cryptosleep sickness
                if (MapInitData.startedFromEntry && Rand.Value < 0.5f)
                {
                    current2.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null);
                }
                // Add the pawns to a second list and add that list to the first
                List<Thing> list2 = new List<Thing>();
                list2.Add(current2);
                list.Add(list2);
            }
            // Create 3 tribal pawns and put them in drop pods crashing
            for (int i = 0; i < 3; i++)
            {
                // Setup faction
                Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Tribe);
                // Generate pawn
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDef.Named("TribalWarrior"), faction, false, 0);
                // Find a location to drop
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 30);
                // Drop a drop pod containg our pawn
                DropShipUtility.MakeDropPodCrashingAt(loc, new DropPodCrashingInfo
                {
                    SingleContainedThing = pawn,
                    openDelay = 180,
                });
            }
            // Debug setting
            bool canInstaDropDuringInit = startedDirectInEditor;
            // Create the drop ship 
            DropShipUtility.CreateDropShipAt(MapGenerator.PlayerStartSpot, list, 110, canInstaDropDuringInit, true, true);
        }

        private static void CreateInitialWorkSettings()
        {
            // Setup initial work settings
            foreach (Pawn current in MapInitData.colonists)
            {
                // Reset everything
                current.workSettings.DisableAll();
            }
            // Loop over each worktype
            foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
            {
                // Setup work priorities
                if (w.alwaysStartActive)
                {
                    foreach (Pawn current2 in
                        from col in MapInitData.colonists
                        where !col.story.WorkTypeIsDisabled(w)
                        select col)
                    {
                        current2.workSettings.SetPriority(w, 3);
                    }
                }
                else
                {
                    bool flag = false;
                    foreach (Pawn current3 in MapInitData.colonists)
                    {
                        if (!current3.story.WorkTypeIsDisabled(w) && current3.skills.AverageOfRelevantSkillsFor(w) >= 6f)
                        {
                            current3.workSettings.SetPriority(w, 3);
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        IEnumerable<Pawn> source =
                            from col in MapInitData.colonists
                            where !col.story.WorkTypeIsDisabled(w)
                            select col;
                        if (source.Any<Pawn>())
                        {
                            Pawn pawn = source.InRandomOrder(null).MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(w));
                            pawn.workSettings.SetPriority(w, 3);
                        }
                        else
                        {
                            if (w.requireCapableColonist)
                            {
                                Log.Error("No colonist could do requireCapableColonist work type " + w);
                            }
                        }
                    }
                }
            }
        }
    }
}
