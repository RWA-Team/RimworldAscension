using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class ScenPart_CrashLanding : ScenPart
    {
        public override void GenerateIntoMap()
        {
            // list of lists to generate after ship crash
            var listsToGenerate = new List<List<Thing>>();

            // colonists list + pet added to that list
            var colonists = Find.GameInitData.startingPawns.Cast<Thing>().ToList();
            listsToGenerate.Add(colonists);

            // Create the ship impactResultThing part
            SkyfallerUtil.MakeShipWreckCrashingAt(MapGenerator.PlayerStartSpot, listsToGenerate);

            IntVec3 dropCell;
            // Create damaged drop pods with dead pawns
            for (var i = 0; i < 2; i++)
            {
                // Generate slaver corpse
                var pawn = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("SpaceSlaverDead"),
                    FactionUtility.DefaultFactionFrom(
                        DefDatabase<PawnKindDef>.GetNamed("SpaceSlaverDead").defaultFactionType));

                // Find a location to drop
                dropCell = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 10);
                // Drop a drop pod containg our pawn
                SkyfallerUtil.MakeDropPodCrashingAt(dropCell, new DropPodInfo { SingleContainedThing = pawn });
            }

            // Create metal debris
            for (var i = 0; i < Rand.RangeInclusive(30, 40); i++)
            {
                // Find a location to drop
                dropCell = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 40);
                SkyfallerUtil.MakeDebrisCrashingAt(dropCell);
            }

            // Create meteorites
            for (var i = 0; i < 10; i++)
            {
                // Find a location to drop
                dropCell = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 20);
                SkyfallerUtil.MakeMeteoriteCrashingAt(dropCell);
            }
        }

        public override void PostMapGenerate()
        {
            MapIniter_NewGame.GiveAllPlayerPawnsThought(ThoughtDefOf.CrashedTogether);
        }
    }
}
