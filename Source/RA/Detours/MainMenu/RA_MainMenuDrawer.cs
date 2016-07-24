using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using UnityEngine;
using UnityEngine.SceneManagement;
using Verse;

namespace RA
{
    public static class RA_MainMenuDrawer
    {
        private static bool anyWorldFiles;
        private static bool anyMapFiles;

        private const float TitleShift = 15f;

        private static readonly Texture2D IconBlog = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Blog");
        private static readonly Texture2D IconForums = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Forums");
        private static readonly Texture2D IconTwitter = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Twitter");
        private static readonly Texture2D IconBook = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Book");
        private static readonly Vector2 PaneSize = new Vector2(450f, 450f);
        private static readonly Vector2 TitleSize = new Vector2(1024f, 200f);
        private static readonly Texture2D TexLudeonLogo = ContentFinder<Texture2D>.Get("UI/HeroArt/LudeonLogoSmall");
        private static readonly Vector2 LudeonLogoSize = new Vector2(200f, 58f);

        //// required for drawing animation
        //public static readonly List<Texture2D> FramesList = ContentFinder<Texture2D>.GetAllInFolder("UI/MainMenu/Animation").ToList();
        //public static int currentFrame = 0;
        //public static int ticksCounter = 0;

        //public static void DrawAnimation(Rect rect, int ticksPerFrame)
        //{
        //    GUI.DrawTexture(rect, FramesList[currentFrame], ScaleMode.StretchToFill, true);

        //    if (ticksCounter >= ticksPerFrame)
        //    {
        //        if (currentFrame >= FramesList.Count - 1)
        //            currentFrame = 0;
        //        else
        //            currentFrame++;

        //        ticksCounter = 0;
        //    }
        //    else
        //    {
        //        ticksCounter++;
        //    }
        //}

        public static void MainMenuOnGUI()
        {
            anyWorldFiles = GenFilePaths.AllWorldFiles.Any();
            anyMapFiles = GenFilePaths.AllSavedGameFiles.Any();

            VersionControl.DrawInfoInCorner();
            // abstract rect in the window center
            var initialRect = new Rect((Screen.width - PaneSize.x) / 2f, (Screen.height - PaneSize.y) / 2f, PaneSize.x, PaneSize.y);
            initialRect.y += 50f;
            //initialRect.x = Screen.width - initialRect.width - 30f;

            // change title rect size and position based on screen size
            var adjust = TitleSize;
            if (adjust.x > Screen.width)
            {
                adjust *= Screen.width / adjust.x;
            }
            adjust *= 0.7f;

            // game title
            var titleRect = new Rect((Screen.width - adjust.x) / 2f + TitleShift, initialRect.y - adjust.y + TitleShift, adjust.x, adjust.y);
            //titleRect.x = Screen.width - adjust.x - 50f;
            GUI.DrawTexture(titleRect, CommonTextures.MainMenuTitle, ScaleMode.StretchToFill, true);

            // tribute to tynan under the main game title
            var creditRect = titleRect;
            creditRect.y += titleRect.height;
            creditRect.xMax -= 55f;
            creditRect.height = 30f;
            creditRect.y += 3f;
            var text = "MainPageCredit".Translate();
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperRight;
            if (Screen.width < 990)
            {
                var position = creditRect;
                position.xMin = position.xMax - Text.CalcSize(text).x;
                position.xMin -= 4f;
                position.xMax += 4f;
                GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                GUI.DrawTexture(position, BaseContent.WhiteTex);
                GUI.color = Color.white;
            }
            Widgets.Label(creditRect, text);

            // logo in the upper right corner
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            var logoRect = new Rect(Screen.width - 8 - LudeonLogoSize.x, 8f, LudeonLogoSize.x, LudeonLogoSize.y);
            GUI.DrawTexture(logoRect, TexLudeonLogo, ScaleMode.StretchToFill, true);
            GUI.color = Color.white;

            // various links
            var linksRect = new Rect(Screen.width - 8 - 150f, logoRect.yMax + 8f, 150f, 500f);
            DoMainMenuLinks(linksRect, anyWorldFiles, anyMapFiles);

            // main menu buttons
            var buttonsRect = initialRect;
            buttonsRect.height += 100f;
            buttonsRect.width = 175f;
            buttonsRect.y += 100f;
            buttonsRect.x = (Screen.width - buttonsRect.width) / 2;
            DoMainMenuButtons(buttonsRect, anyWorldFiles, anyMapFiles);

            // animation
            //Rect animationRect = new Rect(initialRect.x - 700f, initialRect.y, 727f, 346f);
            //DrawAnimation(animationRect, 5);
        }

        public static void DoMainMenuLinks(Rect rect, bool anyWorldFiles, bool anyMapFiles, Action backToGameButtonAction = null)
        {
            //Widgets.DrawWindowBackground(rect);
            Text.Font = GameFont.Small;
            var list = new List<ListableOption>();
            ListableOption item = new ListableOption_WebLink("FictionPrimer".Translate(), "https://docs.google.com/document/d/1pIZyKif0bFbBWten4drrm7kfSSfvBoJPgG9-ywfN8j8/pub", IconBlog);
            list.Add(item);
            item = new ListableOption_WebLink("LudeonBlog".Translate(), "http://ludeon.com/blog", IconBlog);
            list.Add(item);
            item = new ListableOption_WebLink("Forums".Translate(), "http://ludeon.com/forums", IconForums);
            list.Add(item);
            item = new ListableOption_WebLink("OfficialWiki".Translate(), "http://rimworldwiki.com", IconBlog);
            list.Add(item);
            item = new ListableOption_WebLink("TynansTwitter".Translate(), "https://twitter.com/TynanSylvester", IconTwitter);
            list.Add(item);
            item = new ListableOption_WebLink("TynansDesignBook".Translate(), "http://tynansylvester.com/book", IconBook);
            list.Add(item);
            item = new ListableOption_WebLink("HelpTranslate".Translate(), "http://ludeon.com/forums/index.php?topic=2933.0", IconForums);
            list.Add(item);
            OptionListingUtility.DrawOptionListing(rect, list);
            // TODO: check how to fix english lang BadText
            //GUI.BeginGroup(rect);
            //if (Current.ProgramState == ProgramState.Entry && Widgets.ButtonImage(new Rect(0f, num + 10f, 64f, 32f), LanguageDatabase.activeLanguage.icon))
            //{
            //    var list3 = new List<FloatMenuOption>();
            //    foreach (var current in LanguageDatabase.AllLoadedLanguages)
            //    {
            //        var localLang = current;
            //        list3.Add(new FloatMenuOption(localLang.FriendlyNameNative, delegate
            //        {
            //            LanguageDatabase.SelectLanguage(localLang);
            //            Prefs.Save();
            //        }));
            //    }
            //    Find.WindowStack.Add(new FloatMenu(list3));
            //}
            //GUI.EndGroup();
        }

        public static void DoMainMenuButtons(Rect rect, bool anyWorldFiles, bool anyMapFiles, Action backToGameButtonAction = null)
        {
            Text.Font = GameFont.Small;
            var list = new List<ListableOption>();
            ListableOption item;
            if (Current.ProgramState == ProgramState.Entry)
            {
                item = new ListableOption("Continue", delegate
                {
                    // reloads all play data and defs
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        PlayDataLoader.ClearAllPlayData();
                        PlayDataLoader.LoadAllPlayData();
                    }, "LoadingLongEvent", true, null);

                    Action preLoadLevelAction = delegate
                    {
                        Find.GameInitData.ResetWorldRelatedMapInitData();
                        Find.GameInitData.mapToLoad = Path.GetFileNameWithoutExtension(new SaveFileInfo(GenFilePaths.AllSavedGameFiles.FirstOrDefault()).FileInfo.Name);
                    };
                    LongEventHandler.QueueLongEvent(preLoadLevelAction, "Gameplay", "LoadingLongEvent", true, null);
                });
                list.Add(item);
                if (Prefs.DevMode)
                {
                    item = new ListableOption("Quick Start", delegate
                    {
                        // reloads all play data and defs
                        LongEventHandler.QueueLongEvent(delegate
                        {
                            PlayDataLoader.ClearAllPlayData();
                            PlayDataLoader.LoadAllPlayData();
                        }, "LoadingLongEvent", true, null);

                        SceneManager.LoadScene("Gameplay");

                        Find.Root.Start();
                    });
                    list.Add(item);
                }
                if (anyWorldFiles)
                {
                    item = new ListableOption("NewColony".Translate(), delegate
                    {
                        // reloads all play data and defs
                        LongEventHandler.QueueLongEvent(delegate
                        {
                            PlayDataLoader.ClearAllPlayData();
                            PlayDataLoader.LoadAllPlayData();
                        }, "LoadingLongEvent", true, null);

                        Find.WindowStack.Add(new Page_SelectScenario());
                    });
                    list.Add(item);
                }
            }
            if (Current.ProgramState == ProgramState.MapPlaying)
            {
                if (backToGameButtonAction != null)
                {
                    item = new ListableOption("BackToGame".Translate(), backToGameButtonAction);
                    list.Add(item);
                }
                item = new ListableOption("Save".Translate(), delegate
                {
                    CloseMainTab();
                    Find.WindowStack.Add(new Dialog_MapList_Save());
                });
                list.Add(item);
            }
            if (anyMapFiles)
            {
                item = new ListableOption("Load".Translate(), delegate
                {
                    // reloads all play data and defs
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        PlayDataLoader.ClearAllPlayData();
                        PlayDataLoader.LoadAllPlayData();
                    }, "LoadingLongEvent", true, null);

                    CloseMainTab();
                    Find.WindowStack.Add(new Dialog_MapList_Load());
                });
                list.Add(item);
            }
            item = new ListableOption("Options".Translate(), delegate
            {
                CloseMainTab();
                Find.WindowStack.Add(new Dialog_Options());
            });
            list.Add(item);
            if (Current.ProgramState == ProgramState.Entry)
            {
                item = new ListableOption("Mods".Translate(), delegate
                {
                    Find.WindowStack.Add(new Page_ModsConfig());
                });
                list.Add(item);
                item = new ListableOption("Credits".Translate(), delegate
                {
                    Find.WindowStack.Add(new Screen_Credits());
                });
                list.Add(item);
            }
            if (Current.ProgramState == ProgramState.MapPlaying)
            {
                Action action = delegate
                {
                    Find.WindowStack.Add(new Dialog_Confirm("ConfirmQuit".Translate(), delegate
                    {
                        SceneManager.LoadScene("Entry");
                    }, true));
                };
                item = new ListableOption("QuitToMainMenu".Translate(), action);
                list.Add(item);
                Action action2 = delegate
                {
                    Find.WindowStack.Add(new Dialog_Confirm("ConfirmQuit".Translate(), Root.Shutdown, true));
                };
                item = new ListableOption("QuitToOS".Translate(), action2);
                list.Add(item);
            }
            else
            {
                item = new ListableOption("QuitToOS".Translate(), Root.Shutdown);
                list.Add(item);
            }
            OptionListingUtility.DrawOptionListing(rect, list);
        }

        public static void CloseMainTab()
        {
            if (Current.ProgramState == ProgramState.MapPlaying)
            {
                Find.MainTabsRoot.EscapeCurrentTab(false);
            }
        }
    }
}
