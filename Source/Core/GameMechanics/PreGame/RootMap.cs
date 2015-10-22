using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RA
{
    public class RA_RootMap : RootMap
    {
        public static bool globalInitDone;

        public override void Start()
        {
            this.BaseStart();
            if (MapInitData.mapToLoad.NullOrEmpty())
            {
                MapIniter_NewGame.InitNewGeneratedMap();
            }
            else
            {
                MapIniter_LoadFromFile.InitMapFromFile(MapInitData.mapToLoad);
            }
        }

        public virtual void BaseStart()
        {
            SteamManager.InitIfNeeded();
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs != null && commandLineArgs.Length > 1)
            {
                Log.Message("Command line arguments: " + GenText.ToSpaceList(commandLineArgs.Skip(1)));
            }
            if (!RA_RootMap.globalInitDone)
            {
                VersionControl.LogVersionNumber();
                Application.targetFrameRate = 60;
                Prefs.Init();
                RA_RootMap.globalInitDone = true;
            }
            Find.ResetReferences();
            if (!PlayDataLoader.loaded)
            {
                PlayDataLoader.LoadAllPlayData(false);
            }
            ActiveTutorNoteManager.CloseAll();
            this.realTime = new RealTime();
            this.soundRoot = new SoundRoot();
            if (Application.loadedLevelName == "Gameplay")
            {
                this.uiRoot = new UIRoot_Map();
            }
            else if (Application.loadedLevelName == "Entry")
            {
                this.uiRoot = new UIRoot_Entry();
            }
            this.uiRoot.Init();
        }
    }
}