using System.Collections.Generic;
using System.Linq;
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
            foreach (var current in MapInitData.colonists)
            {
                current.SetFactionDirect(Faction.OfColony);
                PawnUtility.AddAndRemoveComponentsAsAppropriate(current);
                current.needs.mood.thoughts.TryGainThought(ThoughtDefOf.NewColonyOptimism);
            }
            CreateInitialWorkSettings();
            var startedDirectInEditor = MapInitData.StartedDirectInEditor;

            // list of lists to generate after ship crash
            var listsToGenerate = new List<List<Thing>>();

            // colonists list added to that list
            var colonists = MapInitData.colonists.Cast<Thing>().ToList();
            listsToGenerate.Add(colonists);

            // Create damaged drop pods with dead pawns
            for (var i = 0; i < MapInitData.colonists.Count; i++)
            {
                // Generate pawn
                var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, FactionUtility.DefaultFactionFrom(FactionDefOf.SpacerHostile));
                // Find a location to drop
                var crashCell = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 30);
                // Drop a drop pod containg our pawn
                SkyfallerUtility.MakeDropPodCrashingAt(crashCell, new DropPodInfo
                {
                    SingleContainedThing = pawn,
                    openDelay = 180
                });
            }

            // Create the ship wreck with colonists
            SkyfallerUtility.MakeShipWreckCrashingAt(MapGenerator.PlayerStartSpot, listsToGenerate, 110, startedDirectInEditor);
        }

        public static void CreateInitialWorkSettings()
        {
            foreach (var current in MapInitData.colonists)
            {
                current.workSettings.DisableAll();
            }
            foreach (var w in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (w.alwaysStartActive)
                {
                    foreach (var current2 in from col in MapInitData.colonists
                                              where !col.story.WorkTypeIsDisabled(w)
                                              select col)
                    {
                        current2.workSettings.SetPriority(w, 3);
                    }
                }
                else
                {
                    var flag = false;
                    foreach (var current3 in MapInitData.colonists)
                    {
                        if (!current3.story.WorkTypeIsDisabled(w) && current3.skills.AverageOfRelevantSkillsFor(w) >= 6f)
                        {
                            current3.workSettings.SetPriority(w, 3);
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        var source = from col in MapInitData.colonists
                                                   where !col.story.WorkTypeIsDisabled(w)
                                                   select col;
                        if (source.Any())
                        {
                            var pawn = source.InRandomOrder().MaxBy(pawn1 => pawn1.skills.AverageOfRelevantSkillsFor(w));
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