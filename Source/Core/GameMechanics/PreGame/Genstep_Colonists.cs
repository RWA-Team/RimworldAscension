using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RA
{
    public class Genstep_Colonists : Genstep
    {
        public const int NumStartingMealsPerColonist = 10;
        public const int NumStartingMedPacksPerColonist = 6;

        public override void Generate()
        {
            foreach (Pawn current in MapInitData.colonists)
            {
                current.SetFactionDirect(Faction.OfColony);
                PawnUtility.AddAndRemoveComponentsAsAppropriate(current);
                current.needs.mood.thoughts.TryGainThought(ThoughtDefOf.NewColonyOptimism);
            }
            CreateInitialWorkSettings();
            bool startedDirectInEditor = MapInitData.StartedDirectInEditor;

            // list of lists to generate after ship crash
            List<List<Thing>> listsToGenerate = new List<List<Thing>>();

            // colonists list added to that list
            List<Thing> colonists = new List<Thing>();
            foreach (Pawn pawn in MapInitData.colonists)
                colonists.Add(pawn);
            listsToGenerate.Add(colonists);

            // Create damaged drop pods with dead tribals
            for (int i = 0; i < MapInitData.colonists.Count; i++)
            {
                // Setup faction
                Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Tribe);
                // Generate pawn
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDef.Named("TribalWarrior"), faction, false, 0);
                // Find a location to drop
                IntVec3 crashCell = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 30);
                // Drop a drop pod containg our pawn
                SkyfallerUtility.MakeDropPodCrashingAt(crashCell, new DropPodInfo
                {
                    SingleContainedThing = pawn,
                    openDelay = 180,
                });
            }

            // Create the ship wreck with colonists
            SkyfallerUtility.MakeShipWreckCrashingAt(MapGenerator.PlayerStartSpot, listsToGenerate, 110, startedDirectInEditor);
        }

        public static void CreateInitialWorkSettings()
        {
            foreach (Pawn current in MapInitData.colonists)
            {
                current.workSettings.DisableAll();
            }
            foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (w.alwaysStartActive)
                {
                    foreach (Pawn current2 in from col in MapInitData.colonists
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
                        IEnumerable<Pawn> source = from col in MapInitData.colonists
                                                   where !col.story.WorkTypeIsDisabled(w)
                                                   select col;
                        if (source.Any<Pawn>())
                        {
                            Pawn pawn = source.InRandomOrder(null).MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(w));
                            pawn.workSettings.SetPriority(w, 3);
                        }
                        else if (w.requireCapableColonist)
                        {
                            Log.Error("No colonist could do requireCapableColonist work type " + w);
                        }
                    }
                }
            }
        }
    }
}
