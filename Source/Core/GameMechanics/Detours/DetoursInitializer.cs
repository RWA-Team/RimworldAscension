using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace RA
{
    public class DetoursInitializer : ITab
    {
        public static List<string> detoured = new List<string>();
        public static List<string> destinations = new List<string>();

        // ITab requirement
        protected override void FillTab() { }

        public DetoursInitializer()
        {
            if (Prefs.DevMode)
                Log.Message("Detours initialized");

            // detour RimWorld.ThingSelectionUtility.SelectableNow
            var vanillaSelectableNow = typeof(ThingSelectionUtility).GetMethod("SelectableNow", BindingFlags.Static | BindingFlags.Public);
            var newSelectableNow = typeof(RA_ThingSelectionUtility).GetMethod("SelectableNow", BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaSelectableNow, newSelectableNow);

            // detour Verse.MapInitData.GenerateDefaultColonistsWithFaction
            var vanillaInitNewGeneratedMap = typeof(MapIniter_NewGame).GetMethod("InitNewGeneratedMap", BindingFlags.Static | BindingFlags.Public);
            var newInitNewGeneratedMap = typeof(RA_MapIniter_NewGame).GetMethod("InitNewGeneratedMap", BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaInitNewGeneratedMap, newInitNewGeneratedMap);
            
            #region MAINMENU

            // detour RimWorld.MainMenuDrawer.MainMenuOnGUI
            var vanillaDoMainMenuButtons = typeof(MainMenuDrawer).GetMethod("MainMenuOnGUI", BindingFlags.Static | BindingFlags.Public);
            var newDoMainMenuButtons = typeof(RA_MainMenuDrawer).GetMethod("MainMenuOnGUI", BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaDoMainMenuButtons, newDoMainMenuButtons);

            // detour RimWorld.UI_BackgroundMain.BackgroundOnGUI
            var vanillaBackgroundOnGUI = typeof(UI_BackgroundMain).GetMethod("BackgroundOnGUI", BindingFlags.Instance | BindingFlags.Public);
            var newBackgroundOnGUI = typeof(RA_UI_BackgroundMain).GetMethod("BackgroundOnGUI", BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaBackgroundOnGUI, newBackgroundOnGUI);

            #endregion

            #region DIPLOMACY

            // allow AffectGoodwillWith with hidden factions
            // detour RimWorld.Faction.AffectGoodwillWith
            var vanillaAffectGoodwillWith = typeof(Faction).GetMethod("AffectGoodwillWith", BindingFlags.Instance | BindingFlags.Public);
            var newAffectGoodwillWith = typeof(RA_Faction).GetMethod("AffectGoodwillWith", BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaAffectGoodwillWith, newAffectGoodwillWith);

            // allow Notify_MemberCaptured with hidden factions
            // detour RimWorld.Faction.Notify_MemberCaptured
            var vanillaNotify_MemberCaptured = typeof(Faction).GetMethod("Notify_MemberCaptured", BindingFlags.Instance | BindingFlags.Public);
            var newNotify_MemberCaptured = typeof(RA_Faction).GetMethod("Notify_MemberCaptured", BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaNotify_MemberCaptured, newNotify_MemberCaptured);

            // allow SetHostileTo with hidden factions
            // detour RimWorld.Faction.SetHostileTo
            var vanillaSetHostileTo = typeof(Faction).GetMethod("SetHostileTo", BindingFlags.Instance | BindingFlags.Public);
            var newSetHostileTo = typeof(RA_Faction).GetMethod("SetHostileTo", BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaSetHostileTo, newSetHostileTo);

            // removes hidden factions restriction from GenerateFactionsIntoCurrentWorld
            // detour RimWorld.FactionGenerator.GenerateFactionsIntoCurrentWorld
            var vanillaGenerateFactionsIntoCurrentWorld = typeof(FactionGenerator).GetMethod("GenerateFactionsIntoCurrentWorld", BindingFlags.Static | BindingFlags.Public);
            var newGenerateFactionsIntoCurrentWorld = typeof(RA_FactionGenerator).GetMethod("GenerateFactionsIntoCurrentWorld", BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaGenerateFactionsIntoCurrentWorld, newGenerateFactionsIntoCurrentWorld);

            // detour Verse.Pawn.ExitMap
            var vanillaExitMap = typeof(Pawn).GetMethod("ExitMap", BindingFlags.Instance | BindingFlags.Public);
            var newExitMap = typeof(RA_Pawn).GetMethod("ExitMap", BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaExitMap, newExitMap);

            #endregion
        }

        /**
            This is a basic first implementation of the IL method 'hooks' (detours) made possible by RawCode's work;
            https://ludeon.com/forums/index.php?topic=17143.0
            Performs detours, spits out basic logs and warns if a method is detoured multiple times.
        **/
        public static unsafe bool TryDetourFromTo(MethodInfo source, MethodInfo destination)
        {
            // keep track of detours and spit out some messaging
            var sourceString = source.DeclaringType.FullName + "." + source.Name;
            var destinationString = destination.DeclaringType.FullName + "." + destination.Name;

            detoured.Add(sourceString);
            destinations.Add(destinationString);

            if (IntPtr.Size == sizeof(Int64))
            {
                // 64-bit systems use 64-bit absolute address and jumps
                // 12 byte destructive

                // Get function pointers
                var Source_Base = source.MethodHandle.GetFunctionPointer().ToInt64();
                var Destination_Base = destination.MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                var Pointer_Raw_Source = (byte*)Source_Base;

                // Pointer to insert jump address into native code
                var Pointer_Raw_Address = (long*)(Pointer_Raw_Source + 0x02);

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *(Pointer_Raw_Source + 0x00) = 0x48;
                *(Pointer_Raw_Source + 0x01) = 0xB8;
                *Pointer_Raw_Address = Destination_Base; // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                *(Pointer_Raw_Source + 0x0A) = 0xFF;
                *(Pointer_Raw_Source + 0x0B) = 0xE0;
            }
            else
            {
                // 32-bit systems use 32-bit relative offset and jump
                // 5 byte destructive

                // Get function pointers
                var Source_Base = source.MethodHandle.GetFunctionPointer().ToInt32();
                var Destination_Base = destination.MethodHandle.GetFunctionPointer().ToInt32();

                // Native source address
                var Pointer_Raw_Source = (byte*)Source_Base;

                // Pointer to insert jump address into native code
                var Pointer_Raw_Address = (int*)(Pointer_Raw_Source + 1);

                // Jump offset (less instruction size)
                var offset = Destination_Base - Source_Base - 5;

                // Insert 32-bit relative jump into native code
                *Pointer_Raw_Source = 0xE9;
                *Pointer_Raw_Address = offset;
            }

            // done!
            return true;
        }
    }
}