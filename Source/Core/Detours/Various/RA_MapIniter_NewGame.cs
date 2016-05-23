using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RA
{
    public static class RA_MapIniter_NewGame
    {
        // changed initial game start message and colonists count
        public static void InitNewGeneratedMap()
        {
            var str = GenText.ToCommaList(from mod in LoadedModManager.LoadedMods
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
                MapInitData.GenerateDefaultColonistsWithFaction();
                SetColonyFactionIntoWorld();
                MapInitData.mapSize = 150;
            }
            DeepProfiler.Start("Set up map");
            Game.Mode = GameMode.MapInitializing;
            Find.RootMap.curMap = new Map();
            Find.Map.info.Size = new IntVec3(MapInitData.mapSize, 1, MapInitData.mapSize);
            Find.Map.info.worldCoords = MapInitData.landingCoords;
            Find.Map.storyteller = new Storyteller(MapInitData.chosenStoryteller, MapInitData.difficulty);
            if (MapInitData.permadeathMode)
            {
                Find.Map.info.permadeathMode = true;
                Find.Map.info.permadeathModeUniqueName = PermadeathModeUtility.GeneratePermadeathSaveName();
            }
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
            Find.TickManager.gameStartAbsTick = MapIniter_NewGame.StartingAbsTicks;
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
                Find.WindowStack.Notify_GameStartDialogOpened();
                var diaNode = new DiaNode("RA_GameStartDialog".Translate());
                var diaOption = new DiaOption();
                diaOption.resolveTree = true;
                diaOption.playClickSound = false;
                diaNode.options.Add(diaOption);
                var dialog_NodeTree = new Dialog_NodeTree(diaNode);
                dialog_NodeTree.soundClose = SoundDef.Named("GameStartSting");
                dialog_NodeTree.closeAction = delegate
                {
                    Find.MusicManagerMap.ForceSilenceFor(7f);
                    Find.MusicManagerMap.disabled = false;
                    Find.WindowStack.Notify_GameStartDialogClosed();
                };
                Find.WindowStack.Add(dialog_NodeTree);
            }
        }

        // duplicated cause of inaccessablility
        public static void SetColonyFactionIntoWorld()
        {
            MapInitData.colonyFaction.homeSquare = MapInitData.landingCoords;
            Find.FactionManager.Add(MapInitData.colonyFaction);
            FactionGenerator.EnsureRequiredEnemies(MapInitData.colonyFaction);
            MapInitData.colonyFaction = null;
        }

        // duplicated cause of inaccessablility
        public static bool TryLoadNewestWorld()
        {
            var fileInfo = (from wf in SavedWorldsDatabase.AllWorldFiles
                                 orderby wf.LastWriteTime descending
                                 select wf).FirstOrDefault();
            if (fileInfo == null)
            {
                return false;
            }
            var fullName = fileInfo.FullName;
            WorldLoader.LoadWorldFromFile(fullName);
            return true;
        }
    }
}
