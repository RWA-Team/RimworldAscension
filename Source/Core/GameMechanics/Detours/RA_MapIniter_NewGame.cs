using RimWorld;
using RimWorld.Planet;

using System.IO;
using System.Linq;
using Verse;

namespace RA
{
    public static class RA_MapIniter_NewGame
    {
        private const int GameStartHourOfDay = 6;

        public static void InitNewGeneratedMap()
        {
            string str = GenText.ToCommaList(from mod in LoadedModManager.LoadedMods
                                             select mod.name);
            Log.Message("Initializing new game with mods " + str);
            DeepProfiler.Start("InitNewGeneratedMap");
            if (!MapInitData.startedFromEntry)
            {
                Game.Mode = GameMode.Entry;
                if (!TryLoadNewestWorld())
                {
                    WorldGenerator.GenerateWorld();
                }
                MapInitData.ChooseDefaultStoryteller();
                MapInitData.ChooseDefaultDifficulty();
                Rand.RandomizeSeedFromTime();
                MapInitData.ChooseDecentLandingSite();
                GenerateDefaultColonistsWithFaction();
                SetColonyFactionIntoWorld();
                MapInitData.mapSize = 150;
            }
            DeepProfiler.Start("Set up map");
            Game.Mode = GameMode.MapInitializing;
            Find.RootMap.curMap = new Map();
            Find.Map.info.Size = new IntVec3(MapInitData.mapSize, 1, MapInitData.mapSize);
            Find.Map.info.worldCoords = MapInitData.landingCoords;
            Find.Map.storyteller = new Storyteller(MapInitData.chosenStoryteller, MapInitData.difficulty);
            MapIniterUtility.ReinitStaticMapComponents_PreConstruct();
            Find.Map.ConstructComponents();
            MapIniterUtility.ReinitStaticMapComponents_PostConstruct();
            if (MapInitData.startingMonth == Month.Undefined)
            {
                MapInitData.startingMonth = GenTemperature.EarliestMonthInTemperatureRange(16f, 9999f);
                if (MapInitData.startingMonth == Month.Undefined)
                {
                    MapInitData.startingMonth = Month.Jun;
                }
            }
            Find.TickManager.gameStartAbsTick = 300000 * (int)MapInitData.startingMonth + 7500;
            DeepProfiler.End();
            DeepProfiler.Start("Generate contents into map");
            MapGenerator.GenerateContentsIntoCurrentMap(DefDatabase<MapGeneratorDef>.GetRandom());
            DeepProfiler.End();
            Find.AreaManager.InitForNewGame();
            DeepProfiler.Start("Finalize map init");
            MapIniterUtility.FinalizeMapInit();
            DeepProfiler.End();
            DeepProfiler.End();
            Find.CameraMap.JumpTo(MapGenerator.PlayerStartSpot);
            if (MapInitData.startedFromEntry)
            {
                Find.MusicManagerMap.disabled = true;
                DiaNode diaNode = new DiaNode("GameStartDialog".Translate());
                DiaOption diaOption = new DiaOption();
                diaOption.resolveTree = true;
                diaOption.action = delegate
                {
                    Find.MusicManagerMap.ForceSilenceFor(7f);
                    Find.MusicManagerMap.disabled = false;
                };
                diaOption.playClickSound = false;
                diaNode.options.Add(diaOption);
                Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode);
                dialog_NodeTree.soundClose = SoundDef.Named("GameStartSting");
                Find.WindowStack.Add(dialog_NodeTree);
            }
        }

        public static void GenerateDefaultColonistsWithFaction()
        {
            MapInitData.colonyFaction = FactionGenerator.NewColonyFaction();
            do
            {
                MapInitData.colonists.Clear();
                for (int i = 0; i < 5; i++)
                {
                    MapInitData.colonists.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfColony, false, 0));
                }
            }
            while (!MapInitData.AnyoneCanDoRequiredWorks());
        }

        public static void SetColonyFactionIntoWorld()
        {
            MapInitData.colonyFaction.homeSquare = MapInitData.landingCoords;
            Find.FactionManager.Add(MapInitData.colonyFaction);
            FactionGenerator.EnsureRequiredEnemies(MapInitData.colonyFaction);
            MapInitData.colonyFaction = null;
        }

        public static bool TryLoadNewestWorld()
        {
            FileInfo fileInfo = (from wf in SavedWorldsDatabase.AllWorldFiles
                                 orderby wf.LastWriteTime descending
                                 select wf).FirstOrDefault();
            if (fileInfo == null)
            {
                return false;
            }
            string fullName = fileInfo.FullName;
            WorldLoader.LoadWorldFromFile(fullName);
            return true;
        }
    }
}
