using System;
using System.IO;
using System.Linq;
using RimWorld;
using UnityEngine.SceneManagement;
using Verse;

namespace RA
{
    public class RA_UIRoot_Entry : UIRoot_Entry
    {
        // added skipping of steam requirement prompt
        public override void Init()
        {
            UIMenuBackgroundManager.background = new UI_BackgroundMain();
            MainMenuDrawer.Init();
            QuickStarter.CheckQuickStart();

            //if (!SteamManager.Initialized)
            //{
            //    var text = "SteamClientMissing".Translate();
            //    if (Application.isEditor)
            //    {
            //        text =
            //            "(The below message is for players. In the editor, you can continue without Steam, though anything might break.)\n\n" +
            //            text;
            //    }
            //    var dialog_Confirm = new Dialog_Confirm(text, Application.Quit, false, null, false)
            //    {
            //        confirmLabel = "OK".Translate()
            //    };
            //    Find.WindowStack.Add(dialog_Confirm);
            //}
        }

        public static class QuickStarter
        {
            public static bool quickStarted;

            public static bool CheckQuickStart()
            {
                if (!quickStarted && GenScene.InEntryScene)
                {
                    if (GenCommandLine.CommandLineArgPassed("quicktest"))
                    {
                        SceneManager.LoadScene("Map");
                    }

                    if (GenCommandLine.CommandLineArgPassed("quickload"))
                    {
                        var latestSavePath =
                            Path.GetFileNameWithoutExtension(GenFilePaths.AllSavedGameFiles?.FirstOrDefault()?.Name);
                        PreLoadUtility.CheckVersionAndLoad(GenFilePaths.FilePathForSavedGame(latestSavePath),
                            ScribeMetaHeaderUtility.ScribeHeaderMode.Map, delegate
                            {
                                Action preLoadLevelAction = delegate
                                {
                                    Current.Game = new Game {InitData = new GameInitData {mapToLoad = latestSavePath}};
                                };
                                LongEventHandler.QueueLongEvent(preLoadLevelAction, "Map", "LoadingLongEvent", true,
                                    null);
                            });
                    }

                    quickStarted = true;
                }

                return quickStarted;
            }
        }
    }
}