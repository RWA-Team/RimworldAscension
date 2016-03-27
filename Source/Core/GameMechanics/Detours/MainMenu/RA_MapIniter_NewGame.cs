using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RA
{
    public static class RA_MapIniter_NewGame
    {
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
                var diaNode = new DiaNode("GameStartDialog".Translate());
                var diaOption = new DiaOption
                {
                    resolveTree = true,
                    action = delegate
                    {
                        Find.MusicManagerMap.ForceSilenceFor(7f);
                        Find.MusicManagerMap.disabled = false;
                    },
                    playClickSound = false
                };
                diaNode.options.Add(diaOption);
                var dialog_NodeTree = new Dialog_NodeTree(diaNode) {soundClose = SoundDef.Named("GameStartSting")};
                Find.WindowStack.Add(dialog_NodeTree);
            }
        }

        public static void GenerateDefaultColonistsWithFaction()
        {
            MapInitData.colonyFaction = FactionGenerator.NewColonyFaction();
            do
            {
                MapInitData.colonists.Clear();
                for (var i = 0; i < 5; i++)
                {
                    MapInitData.colonists.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfColony));
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
