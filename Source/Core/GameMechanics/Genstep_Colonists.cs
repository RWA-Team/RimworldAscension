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

        public const int CrushingDropPodsCount = 2;

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

            // colonists list + pet added to that list
            var colonists = MapInitData.colonists.Cast<Thing>().ToList();
            colonists.Add(RandomPet());
            listsToGenerate.Add(colonists);

            // Create the ship impactResultThing part
            SkyfallerUtility.MakeShipWreckCrashingAt(MapGenerator.PlayerStartSpot, listsToGenerate, 110,
                startedDirectInEditor);

            // Create damaged drop pods with dead pawns
            for (var i = 0; i < CrushingDropPodsCount; i++)
            {
                // Generate slaver corpse
                var pawn = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("SpaceSlaverDead"),
                    FactionUtility.DefaultFactionFrom(
                        DefDatabase<PawnKindDef>.GetNamed("SpaceSlaverDead").defaultFactionType));

                // Find a location to drop
                IntVec3 dropCell;
                if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(MapGenerator.PlayerStartSpot, 10, out dropCell))
                    dropCell = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 30);

                // Drop a drop pod containg our pawn
                SkyfallerUtility.MakeDropPodCrashingAt(dropCell, new DropPodInfo
                {
                    SingleContainedThing = pawn,
                    openDelay = 180
                });
            }

            // Create damaged drop pods with dead pawns
            for (var i = 0; i < Rand.RangeInclusive(10, 20); i++)
            {
                // Find a location to drop
                IntVec3 dropCell;
                if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(MapGenerator.PlayerStartSpot, 10, out dropCell))
                    dropCell = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 30);

                // Drop a drop pod containg our pawn
                SkyfallerUtility.MakeDebrisCrashingAt(dropCell);
            }
        }

        public static Thing RandomPet()
        {
            var kindDef = (from td in DefDatabase<PawnKindDef>.AllDefs
                where td.race.category == ThingCategory.Pawn && td.RaceProps.petness > 0f
                select td).RandomElementByWeight(td => td.RaceProps.petness);
            var pawn = PawnGenerator.GeneratePawn(kindDef, Faction.OfColony);
            if (pawn.Name == null || pawn.Name.Numerical)
            {
                pawn.Name = NameGenerator.GenerateName(pawn);
            }
            return pawn;
        }

        public static void CreateInitialWorkSettings()
        {
            // disable all worktypes
            foreach (var pawn in MapInitData.colonists)
            {
                pawn.workSettings.DisableAll();
            }

            foreach (var workType in DefDatabase<WorkTypeDef>.AllDefs)
            {
                // if worktype is enabled in any case (like firefighting)
                if (workType.alwaysStartActive)
                {
                    // set priority 3 to all allowed work types
                    foreach (
                        var pawn in
                            MapInitData.colonists.Where(colonist => !colonist.story.WorkTypeIsDisabled(workType)))
                    {
                        pawn.workSettings.SetPriority(workType, 3);
                    }
                }
                else
                {
                    var currentWorkTypeCapableColonistGenerated = false;
                    foreach (var colonist in MapInitData.colonists)
                    {
                        if (!colonist.story.WorkTypeIsDisabled(workType) &&
                            colonist.skills.AverageOfRelevantSkillsFor(workType) >= 6f)
                        {
                            colonist.workSettings.SetPriority(workType, 3);
                            currentWorkTypeCapableColonistGenerated = true;
                        }
                    }
                    if (!currentWorkTypeCapableColonistGenerated)
                    {
                        var pawns = MapInitData.colonists.Where(colonist => !colonist.story.WorkTypeIsDisabled(workType));

                        if (pawns.Any())
                        {
                            var pawn =
                                pawns.InRandomOrder().MaxBy(pawn1 => pawn1.skills.AverageOfRelevantSkillsFor(workType));
                            pawn.workSettings.SetPriority(workType, 3);
                        }
                        else if (workType.requireCapableColonist)
                        {
                            Log.Error("No colonist could do requireCapableColonist work type " + workType);
                        }
                    }
                }
            }
        }
    }
}