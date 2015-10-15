using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RimworldAscension.PreGame
{
    public static class MapInitData
    {
        public const int StartingColonistCount = 5;

        public static bool startedFromEntry;

        public static string mapToLoad;

        public static int mapSize;

        public static DifficultyDef difficulty;

        public static StorytellerDef chosenStoryteller;

        public static IntVec2 landingCoords;

        public static List<Pawn> colonists;

        public static Faction colonyFaction;

        public static Month startingMonth;

        public static int debug_loadsSinceReset;

        public static bool StartedDirectInEditor
        {
            get
            {
                return MapInitData.mapToLoad.NullOrEmpty() && !MapInitData.startedFromEntry;
            }
        }

        static MapInitData()
        {
            MapInitData.colonists = new List<Pawn>();
            MapInitData.colonyFaction = null;
            MapInitData.startingMonth = Month.Undefined;
            MapInitData.debug_loadsSinceReset = 0;
            MapInitData.Reset();
        }

        public static void Reset()
        {
            ThingIDMaker.Clear();
            MapInitData.startedFromEntry = false;
            MapInitData.mapToLoad = null;
            MapInitData.chosenStoryteller = null;
            MapInitData.difficulty = null;
            MapInitData.mapSize = 250;
            MapInitData.landingCoords = IntVec2.Invalid;
            MapInitData.debug_loadsSinceReset = 0;
            MapInitData.colonists.Clear();
            MapInitData.colonyFaction = null;
            MapInitData.startingMonth = Month.Undefined;
        }

        public static void ChooseDecentLandingSite()
        {
            Verse.MapInitData.ChooseDecentLandingSite();
        }

        public static void ChooseDefaultStoryteller()
        {
            MapInitData.chosenStoryteller = DefDatabase<StorytellerDef>.GetNamed("Cassandra", true);
            if (MapInitData.chosenStoryteller == null)
            {
                MapInitData.chosenStoryteller = (from d in DefDatabase<StorytellerDef>.AllDefs
                                                 orderby d.listOrder
                                                 select d).First<StorytellerDef>();
            }
        }

        public static void ChooseDefaultDifficulty()
        {
            MapInitData.difficulty = DefDatabase<DifficultyDef>.GetNamed("Challenge", true);
        }

        public static void GenerateDefaultColonistsWithFaction()
        {
            MapInitData.colonyFaction = FactionGenerator.NewColonyFaction();
            do
            {
                MapInitData.colonists.Clear();
                for (int i = 0; i < StartingColonistCount; i++)
                {
                    MapInitData.colonists.Add(MapInitData.NewGeneratedColonist());
                }
            }
            while (!MapInitData.AnyoneCanDoRequiredWorks());
        }

        public static void Notify_MapInited()
        {
            MapInitData.debug_loadsSinceReset++;
            if (MapInitData.debug_loadsSinceReset > 1)
            {
                Log.Warning("Failed to reset map init params.");
            }
        }

        public static bool AnyoneCanDoRequiredWorks()
        {
            if (MapInitData.colonists.Count == 0)
            {
                return false;
            }
            foreach (WorkTypeDef wt in from w in DefDatabase<WorkTypeDef>.AllDefs
                                       where w.requireCapableColonist
                                       select w)
            {
                if (!MapInitData.colonists.Any((Pawn col) => !col.story.WorkTypeIsDisabled(wt)))
                {
                    return false;
                }
            }
            return true;
        }

        public static Pawn RegenerateStartingColonist(Pawn p)
        {
            Pawn pawn = MapInitData.NewGeneratedColonist();
            MapInitData.colonists[MapInitData.colonists.IndexOf(p)] = pawn;
            return pawn;
        }

        public static void SetColonyFactionIntoWorld()
        {
            MapInitData.colonyFaction.homeSquare = MapInitData.landingCoords;
            Find.FactionManager.Add(MapInitData.colonyFaction);
            FactionGenerator.EnsureRequiredEnemies(MapInitData.colonyFaction);
            MapInitData.colonyFaction = null;
        }

        public static Pawn NewGeneratedColonist()
        {
            return PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfColony, false, 0);
        }
    }
}
