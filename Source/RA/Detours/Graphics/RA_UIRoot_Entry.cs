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
            private static bool quickStarted;

            public static bool CheckQuickStart()
            {
                if (GenCommandLine.CommandLineArgPassed("quicktest") && !quickStarted && GenScene.InEntryScene)
                {
                    quickStarted = true;
                    SceneManager.LoadScene("Map");
                    return true;
                }
                return false;
            }
        }
    }
}
