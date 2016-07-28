using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RA
{
    [StaticConstructorOnStartup]
    public class Initializer
    {
        public static List<string> sourceMethods = new List<string>();
        public static List<string> destMethods = new List<string>();

        static Initializer()
        {
            DoDetours();

            if (Prefs.DevMode)
                Log.Message("Detours initialized");

            LongEventHandler.QueueLongEvent(delegate
            {
                PlayDataLoader.ClearAllPlayData();
                PlayDataLoader.LoadAllPlayData();
            }, "LoadingLongEvent", true, null);
        }

        public static object GetHiddenValue(Type type, object instance, string fieldName, FieldInfo info,
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            if (info == null)
            {
                info = type.GetField(fieldName, flags);
            }

            return info?.GetValue(instance);
        }

        public static void SetHiddenValue(object value, Type type, object instance, string fieldName, FieldInfo info,
                                               BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            if (info == null)
            {
                info = type.GetField(fieldName, flags);
            }

            info?.SetValue(instance, value);
        }
        //This is a basic first implementation of the IL method 'hooks' (detours) made possible by RawCode's work;
        //https://ludeon.com/forums/index.php?topic=17143.0
        //Performs detours, spits out basic logs and warns if a method is sourceMethods multiple times.
        public static unsafe bool TryDetourFromTo(MethodInfo source, MethodInfo destination)
        {
            // keep track of detours and spit out some messaging
            var sourceString = source.DeclaringType.FullName + "." + source.Name;
            var destinationString = destination.DeclaringType.FullName + "." + destination.Name;

            sourceMethods.Add(sourceString);
            destMethods.Add(destinationString);

            if (IntPtr.Size == sizeof (Int64))
            {
                // 64-bit systems use 64-bit absolute address and jumps
                // 12 byte destructive

                // Get function pointers
                var Source_Base = source.MethodHandle.GetFunctionPointer().ToInt64();
                var Destination_Base = destination.MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                var Pointer_Raw_Source = (byte*) Source_Base;

                // Pointer to insert jump address into native code
                var Pointer_Raw_Address = (long*) (Pointer_Raw_Source + 0x02);

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *(Pointer_Raw_Source + 0x00) = 0x48;
                *(Pointer_Raw_Source + 0x01) = 0xB8;
                *Pointer_Raw_Address = Destination_Base;
                // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
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
                var Pointer_Raw_Source = (byte*) Source_Base;

                // Pointer to insert jump address into native code
                var Pointer_Raw_Address = (int*) (Pointer_Raw_Source + 1);

                // Jump offset (less instruction size)
                var offset = Destination_Base - Source_Base - 5;

                // Insert 32-bit relative jump into native code
                *Pointer_Raw_Source = 0xE9;
                *Pointer_Raw_Address = offset;
            }

            // done!
            return true;
        }

        public static void DoDetours()
        {
            #region AI
            
            // send Notify_PawnTookDamage signal to the LordToil
            var vanillaNotify_PawnTookDamage = typeof(Lord).GetMethod("Notify_PawnTookDamage",
                BindingFlags.Instance | BindingFlags.Public);
            var newNotify_PawnTookDamage = typeof(RA_Lord).GetMethod("Notify_PawnTookDamage",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaNotify_PawnTookDamage, newNotify_PawnTookDamage);

            #endregion

            #region COMBAT

            // flat damage reduction armor system and armor penetration
            var vanillaApplyDamagePartial = typeof(DamageWorker_AddInjury).GetMethod("ApplyDamagePartial",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var newApplyDamagePartial = typeof(RA_DamageWorker_AddInjury).GetMethod("ApplyDamagePartial",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaApplyDamagePartial, newApplyDamagePartial);

            // flat damage reduction armor system and armor penetration
            var vanillaCheckDuplicateDamageToOuterParts =
                typeof(DamageWorker_AddInjury).GetMethod("CheckDuplicateDamageToOuterParts",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var newCheckDuplicateDamageToOuterParts =
                typeof(RA_DamageWorker_AddInjury).GetMethod("CheckDuplicateDamageToOuterParts",
                    BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaCheckDuplicateDamageToOuterParts, newCheckDuplicateDamageToOuterParts);

            // flat damage reduction armor system and armor penetration
            var vanillaCheckPropagateDamageToInnerSolidParts =
                typeof(DamageWorker_AddInjury).GetMethod("CheckPropagateDamageToInnerSolidParts",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var newCheckPropagateDamageToInnerSolidParts =
                typeof(RA_DamageWorker_AddInjury).GetMethod("CheckPropagateDamageToInnerSolidParts",
                    BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaCheckPropagateDamageToInnerSolidParts, newCheckPropagateDamageToInnerSolidParts);

            #endregion

            #region CONTAINERS

            // allows to place items to container stacks
            var vanillaCompTickRare = typeof(CompRottable).GetMethod("CompTickRare",
                BindingFlags.Instance | BindingFlags.Public);
            var newCompTickRare = typeof(RA_CompRottable).GetMethod("CompTickRare",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaCompTickRare, newCompTickRare);

            // allows to place items to container stacks
            var vanillaTryPlaceDirect = typeof(GenPlace).GetMethod("TryPlaceDirect",
                BindingFlags.Static | BindingFlags.NonPublic);
            var newTryPlaceDirect = typeof(RA_GenPlace).GetMethod("TryPlaceDirect",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaTryPlaceDirect, newTryPlaceDirect);

            // allows to haul items to container stacks
            var vanillaHaulMaxNumToCellJob = typeof(HaulAIUtility).GetMethod("HaulMaxNumToCellJob",
                BindingFlags.Static | BindingFlags.Public);
            var newHaulMaxNumToCellJob = typeof(RA_HaulAIUtility).GetMethod("HaulMaxNumToCellJob",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaHaulMaxNumToCellJob, newHaulMaxNumToCellJob);

            // allows to pass checks for item stacking in cells with containers
            var vanillaNoStorageBlockersIn = typeof(StoreUtility).GetMethod("NoStorageBlockersIn",
                BindingFlags.Static | BindingFlags.NonPublic);
            var newNoStorageBlockersIn = typeof(RA_StoreUtility).GetMethod("NoStorageBlockersIn",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaNoStorageBlockersIn, newNoStorageBlockersIn);

            #endregion

            #region DESIGNATORS

            // changes initial log window size
            var vanillaProcessInput = typeof(Designator_Build).GetMethod("ProcessInput",
               BindingFlags.Instance | BindingFlags.Public);
            var newProcessInput = typeof(RA_Designator_Build).GetMethod("ProcessInput",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaProcessInput, newProcessInput);

            // TODO
            var vanillaDrawPanelReadout = typeof(Designator_Build).GetMethod("DrawPanelReadout",
                BindingFlags.Instance | BindingFlags.Public);
            var newDrawPanelReadout = typeof(RA_Designator_Build).GetMethod("DrawPanelReadout",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaDrawPanelReadout, newDrawPanelReadout);

            // TODO
            var vanillaDrawMouseAttachments = typeof(Designator_Build).GetMethod("DrawMouseAttachments",
                BindingFlags.Instance | BindingFlags.Public);
            var newDrawMouseAttachments = typeof(RA_Designator_Build).GetMethod("DrawMouseAttachments",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaDrawMouseAttachments, newDrawMouseAttachments);

            // TODO
            //var vanillaGroupsWith = typeof(Designator_Build).GetMethod("GroupsWith",
            //    BindingFlags.Instance | BindingFlags.Public);
            //var newGroupsWith = typeof(RA_Designator_Build).GetMethod("GroupsWith",
            //    BindingFlags.Instance | BindingFlags.Public);
            //TryDetourFromTo(vanillaGroupsWith, newGroupsWith);

            // make plant cut designator work on plants with "Cut" harvest tag only
            var vanillaCanDesignateThing = typeof(Designator_Plants).GetMethod("CanDesignateThing",
                BindingFlags.Instance | BindingFlags.Public);
            var newCanDesignateThing = typeof(RA_Designator_Plants).GetMethod("CanDesignateThing",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaCanDesignateThing, newCanDesignateThing);

            // make plant harvest designator work on plants with "Harvest" harvest tag only
            var vanillaCanDesignateThing2 = typeof(Designator_PlantsHarvest).GetMethod("CanDesignateThing",
                BindingFlags.Instance | BindingFlags.Public);
            var newCanDesignateThing2 = typeof(RA_Designator_PlantsHarvest).GetMethod("CanDesignateThing",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaCanDesignateThing2, newCanDesignateThing2);

            // make mine designator work after special research is made
            var vanillaCanDesignateThing3 = typeof(Designator_Mine).GetMethod("CanDesignateThing",
                BindingFlags.Instance | BindingFlags.Public);
            var newCanDesignateThing3 = typeof(RA_Designator_Mine).GetMethod("CanDesignateThing",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaCanDesignateThing3, newCanDesignateThing3);

            #endregion

            #region JOBS

            // fixes bug for decreased carry capacity
            var vanillaJumpToCollectNextIntoHandsForBill =
                typeof(JobDriver_DoBill).GetMethod("JumpToCollectNextIntoHandsForBill",
                    BindingFlags.Static | BindingFlags.NonPublic);
            var newJumpToCollectNextIntoHandsForBill =
                typeof(RA_JobDriver_DoBill).GetMethod("JumpToCollectNextIntoHandsForBill",
                    BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaJumpToCollectNextIntoHandsForBill, newJumpToCollectNextIntoHandsForBill);

            // changed how prodcut stuff type is determined and made this toil assign production cost for the Thing to the CompCraftedValue
            var vanillaFinishRecipeAndStartStoringProduct =
                typeof(Toils_Recipe).GetMethod("FinishRecipeAndStartStoringProduct",
                    BindingFlags.Static | BindingFlags.Public);
            var newFinishRecipeAndStartStoringProduct =
                typeof(RA_Toils_Recipe).GetMethod("FinishRecipeAndStartStoringProduct",
                    BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaFinishRecipeAndStartStoringProduct, newFinishRecipeAndStartStoringProduct);

            // made recipe decide what result stuff to make based on defaultIngredientFilter as blocking Stuff types one
            var vanillaGetDominantIngredient = typeof(Toils_Recipe).GetMethod("GetDominantIngredient",
                BindingFlags.Static | BindingFlags.NonPublic);
            var newGetDominantIngredient = typeof(RA_Toils_Recipe).GetMethod("GetDominantIngredient",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaGetDominantIngredient, newGetDominantIngredient);

            // added support for fuel burners
            var vanillaDoRecipeWork = typeof(Toils_Recipe).GetMethod("DoRecipeWork",
                BindingFlags.Static | BindingFlags.Public);
            var newDoRecipeWork = typeof(RA_Toils_Recipe).GetMethod("DoRecipeWork",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaDoRecipeWork, newDoRecipeWork);

            // skip message prompt for research recipes
            var vanillaNotify_IterationCompleted = typeof(Bill_Production).GetMethod("Notify_IterationCompleted",
                BindingFlags.Instance | BindingFlags.Public);
            var newNotify_IterationCompleted = typeof(RA_Bill_Production).GetMethod("Notify_IterationCompleted",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaNotify_IterationCompleted, newNotify_IterationCompleted);

            #endregion

            #region TRADE

            // made trade system accept items for trade in trade zones around trade centers
            var vanillaColonyThingsWillingToBuy = typeof (Pawn).GetProperty("ColonyThingsWillingToBuy",
                BindingFlags.Instance | BindingFlags.Public);
            var vanillaColonyThingsWillingToBuy_Getter = vanillaColonyThingsWillingToBuy.GetGetMethod();
            var newColonyThingsWillingToBuy = typeof (RA_Pawn).GetProperty("ColonyThingsWillingToBuy",
                BindingFlags.Instance | BindingFlags.Public);
            var newColonyThingsWillingToBuy_Getter = newColonyThingsWillingToBuy.GetGetMethod();
            TryDetourFromTo(vanillaColonyThingsWillingToBuy_Getter, newColonyThingsWillingToBuy_Getter);

            // made trade system offer items for trade from tradeStock in current trade center
            var vanillaGoods = typeof (Pawn).GetProperty("Goods", BindingFlags.Instance | BindingFlags.Public);
            var vanillaGoods_Getter = vanillaGoods.GetGetMethod();
            var newGoods = typeof (RA_Pawn).GetProperty("Goods", BindingFlags.Instance | BindingFlags.Public);
            var newGoods_Getter = newGoods.GetGetMethod();
            TryDetourFromTo(vanillaGoods_Getter, newGoods_Getter);

            // resolve trade by assigning hauling jobs
            var vanillaResolveTrade = typeof (Tradeable).GetMethod("ResolveTrade",
                BindingFlags.Instance | BindingFlags.Public);
            var newResolveTrade = typeof (RA_Tradeable).GetMethod("ResolveTrade",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaResolveTrade, newResolveTrade);

            // resolve trade for pawns
            var vanillaResolveTradePawn = typeof (Tradeable_Pawn).GetMethod("ResolveTrade",
                BindingFlags.Instance | BindingFlags.Public);
            var newResolveTradePawn = typeof (RA_Tradeable_Pawn).GetMethod("ResolveTrade",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaResolveTradePawn, newResolveTradePawn);

            // adjusted price calculation
            var vanillaPriceFor = typeof (Tradeable).GetMethod("PriceFor", BindingFlags.Instance | BindingFlags.Public);
            var newPriceFor = typeof (RA_Tradeable).GetMethod("PriceFor", BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaPriceFor, newPriceFor);

            // make trade sessions individual per trade center
            var vanillaSetupWith = typeof (TradeSession).GetMethod("SetupWith",
                BindingFlags.Static | BindingFlags.Public);
            var newSetupWith = typeof (RA_TradeSession).GetMethod("SetupWith", BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaSetupWith, newSetupWith);

            // assign Pawn_TraderTracker based on pawn caravan role, not mindState.wantsToTradeWithColony
            var vanillaAddAndRemoveDynamicComponents =
                typeof (PawnComponentsUtility).GetMethod("AddAndRemoveDynamicComponents",
                    BindingFlags.Static | BindingFlags.Public);
            var newAddAndRemoveDynamicComponents =
                typeof (RA_PawnComponentsUtility).GetMethod("AddAndRemoveDynamicComponents",
                    BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaAddAndRemoveDynamicComponents, newAddAndRemoveDynamicComponents);

            #endregion

            #region GRAPHICS

            // changes initial log window size
            var vanillaPostOpen = typeof(EditWindow_Log).GetMethod("PostOpen",
                BindingFlags.Instance | BindingFlags.Public);
            var newPostOpen = typeof(RA_EditWindow_Log).GetMethod("PostOpen",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaPostOpen, newPostOpen);

            // added skipping of steam requirement prompt
            var vanillaInit = typeof(UIRoot_Entry).GetMethod("Init",
                BindingFlags.Instance | BindingFlags.Public);
            var newInit = typeof(RA_UIRoot_Entry).GetMethod("Init",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaInit, newInit);

            // detour RimWorld.MainMenuDrawer.MainMenuOnGUI
            var vanillaMainMenuOnGUI = typeof(MainMenuDrawer).GetMethod("MainMenuOnGUI",
                BindingFlags.Static | BindingFlags.Public);
            var newMainMenuOnGUI = typeof(RA_MainMenuDrawer).GetMethod("MainMenuOnGUI",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaMainMenuOnGUI, newMainMenuOnGUI);

            // detour RimWorld.UI_BackgroundMain.BackgroundOnGUI
            var vanillaBackgroundOnGUI = typeof (UI_BackgroundMain).GetMethod("BackgroundOnGUI",
                BindingFlags.Instance | BindingFlags.Public);
            var newBackgroundOnGUI = typeof (RA_UI_BackgroundMain).GetMethod("BackgroundOnGUI",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaBackgroundOnGUI, newBackgroundOnGUI);

            // draws hands on equipment, if corresponding Comp is specified
            var vanillaDrawEquipmentAiming = typeof(PawnRenderer).GetMethod("DrawEquipmentAiming",
                BindingFlags.Instance | BindingFlags.Public);
            var newDrawEquipmentAiming = typeof(RA_PawnRenderer).GetMethod("DrawEquipmentAiming",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaDrawEquipmentAiming, newDrawEquipmentAiming);

            //changed inner graphic extraction for minified things
            var vanillaExtractInnerGraphicFor = typeof(GraphicUtility).GetMethod("ExtractInnerGraphicFor",
                BindingFlags.Static | BindingFlags.Public);
            var newExtractInnerGraphicFor = typeof(RA_GraphicUtility).GetMethod("ExtractInnerGraphicFor",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaExtractInnerGraphicFor, newExtractInnerGraphicFor);

            // changed text align to middle center
            var vanillaButtonTextSubtle = typeof(Widgets).GetMethod("ButtonTextSubtle",
                BindingFlags.Static | BindingFlags.Public);
            var newButtonTextSubtle = typeof(RA_Widgets).GetMethod("ButtonTextSubtle",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaButtonTextSubtle, newButtonTextSubtle);

            // make ProgressBar effecter deteriorate used tools
            var vanillaSubEffectTick = typeof(SubEffecter_ProgressBar).GetMethod("SubEffectTick",
                BindingFlags.Instance | BindingFlags.Public);
            var newSubEffectTick = typeof(RA_SubEffecter_ProgressBar).GetMethod("SubEffectTick",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaSubEffectTick, newSubEffectTick);

            // resolves reference to MainTabWindow_Architect
            var vanillaInfoRect = typeof(ArchitectCategoryTab).GetProperty("InfoRect",
                BindingFlags.Static | BindingFlags.Public);
            var vanillaInfoRect_Getter = vanillaInfoRect.GetGetMethod();
            var newInfoRect = typeof(RA_ArchitectCategoryTab).GetProperty("InfoRect",
                BindingFlags.Static | BindingFlags.Public);
            var newInfoRect_Getter = newInfoRect.GetGetMethod();
            TryDetourFromTo(vanillaInfoRect_Getter, newInfoRect_Getter);

            // resolves reference to MainTabWindow_Architect
            var vanillaDesignationTabOnGUI = typeof(ArchitectCategoryTab).GetMethod("DesignationTabOnGUI",
                BindingFlags.Instance | BindingFlags.Public);
            var newDesignationTabOnGUI = typeof(RA_ArchitectCategoryTab).GetMethod("DesignationTabOnGUI",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaDesignationTabOnGUI, newDesignationTabOnGUI);

            #endregion

            #region DIPLOMACY

            // allow AffectGoodwillWith with hidden factions; make factions hostile when relations are negative
            // detour RimWorld.Faction.AffectGoodwillWith
            //var vanillaAffectGoodwillWith = typeof(Faction).GetMethod("AffectGoodwillWith", BindingFlags.Instance | BindingFlags.Public);
            //var newAffectGoodwillWith = typeof(RA_Faction).GetMethod("AffectGoodwillWith", BindingFlags.Instance | BindingFlags.Public);
            //TryDetourFromTo(vanillaAffectGoodwillWith, newAffectGoodwillWith);

            //// allow Notify_MemberCaptured with hidden factions
            //// detour RimWorld.Faction.Notify_MemberCaptured
            //var vanillaNotify_MemberCaptured = typeof(Faction).GetMethod("Notify_MemberCaptured", BindingFlags.Instance | BindingFlags.Public);
            //var newNotify_MemberCaptured = typeof(RA_Faction).GetMethod("Notify_MemberCaptured", BindingFlags.Instance | BindingFlags.Public);
            //TryDetourFromTo(vanillaNotify_MemberCaptured, newNotify_MemberCaptured);

            //// allow Notify_MemberTookDamage with hidden factions
            //// detour RimWorld.Faction.Notify_MemberTookDamage
            //var vanillaNotify_MemberTookDamage = typeof(Faction).GetMethod("Notify_MemberTookDamage", BindingFlags.Instance | BindingFlags.Public);
            //var newNotify_MemberTookDamage = typeof(RA_Faction).GetMethod("Notify_MemberTookDamage", BindingFlags.Instance | BindingFlags.Public);
            //TryDetourFromTo(vanillaNotify_MemberTookDamage, newNotify_MemberTookDamage);

            //// allow Notify_FactionTick with hidden factions
            //// detour RimWorld.Faction.Notify_FactionTick
            //var vanillaNotify_FactionTick = typeof(Faction).GetMethod("Notify_FactionTick", BindingFlags.Instance | BindingFlags.Public);
            //var newNotify_FactionTick = typeof(RA_Faction).GetMethod("Notify_FactionTick", BindingFlags.Instance | BindingFlags.Public);
            //TryDetourFromTo(vanillaNotify_FactionTick, newNotify_FactionTick);

            // allow SetHostileTo with hidden factions
            // detour RimWorld.Faction.SetHostileTo
            //var vanillaSetHostileTo = typeof(Faction).GetMethod("SetHostileTo", BindingFlags.Instance | BindingFlags.Public);
            //var newSetHostileTo = typeof(RA_Faction).GetMethod("SetHostileTo", BindingFlags.Instance | BindingFlags.Public);
            //TryDetourFromTo(vanillaSetHostileTo, newSetHostileTo);

            // removes hidden factions restriction from GenerateFactionsIntoCurrentWorld
            // detour RimWorld.FactionGenerator.GenerateFactionsIntoCurrentWorld
            //var vanillaGenerateFactionsIntoCurrentWorld = typeof(FactionGenerator).GetMethod("GenerateFactionsIntoCurrentWorld", BindingFlags.Static | BindingFlags.Public);
            //var newGenerateFactionsIntoCurrentWorld = typeof(RA_FactionGenerator).GetMethod("GenerateFactionsIntoCurrentWorld", BindingFlags.Static | BindingFlags.Public);
            //TryDetourFromTo(vanillaGenerateFactionsIntoCurrentWorld, newGenerateFactionsIntoCurrentWorld);

            //// detour Verse.Pawn.ExitMap
            //var vanillaExitMap = typeof(Pawn).GetMethod("ExitMap", BindingFlags.Instance | BindingFlags.Public);
            //var newExitMap = typeof(RA_Pawn).GetMethod("ExitMap", BindingFlags.Instance | BindingFlags.Public);
            //TryDetourFromTo(vanillaExitMap, newExitMap);

            #endregion

            #region VARIOUS

            var vanillaSelectableNow = typeof(ThingSelectionUtility).GetMethod("SelectableNow",
                BindingFlags.Static | BindingFlags.Public);
            var newSelectableNow = typeof(RA_ThingSelectionUtility).GetMethod("SelectableNow",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaSelectableNow, newSelectableNow);

            // added new def generators and removed redundant
            var vanillaGenerateImpliedDefs_PreResolve = typeof(DefGenerator).GetMethod(
                "GenerateImpliedDefs_PreResolve",
                BindingFlags.Static | BindingFlags.Public);
            var newGenerateImpliedDefs_PreResolve = typeof(RA_DefGenerator).GetMethod("GenerateImpliedDefs_PreResolve",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaGenerateImpliedDefs_PreResolve, newGenerateImpliedDefs_PreResolve);

            // changed butcher yields
            var vanillaButcherProducts = typeof(Pawn).GetMethod("ButcherProducts",
                BindingFlags.Instance | BindingFlags.Public);
            var newButcherProducts = typeof(RA_Pawn).GetMethod("ButcherProducts",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaButcherProducts, newButcherProducts);

            // special spawns for skyfaller impact explosion (otherwise things are damaged by eplosion even if spawned after that, but without delay)
            var vanillaTrySpawnExplosionThing = typeof(Explosion).GetMethod("TrySpawnExplosionThing",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var newTrySpawnExplosionThing = typeof(RA_Explosion).GetMethod("TrySpawnExplosionThing",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaTrySpawnExplosionThing, newTrySpawnExplosionThing);

            // tries to assign existing stuff type, instead of some random one, as default
            var vanillaDefaultStuffFor = typeof(GenStuff).GetMethod("DefaultStuffFor",
                BindingFlags.Static | BindingFlags.Public);
            var newDefaultStuffFor = typeof(RA_GenStuff).GetMethod("DefaultStuffFor",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaDefaultStuffFor, newDefaultStuffFor);

            //// CR aim pie
            //var vanillaStanceDraw = typeof(Stance_Warmup).GetMethod("StanceDraw", BindingFlags.Instance | BindingFlags.Public);
            //var newStanceDraw = typeof(RA_Stance_Warmup).GetMethod("StanceDraw", BindingFlags.Instance | BindingFlags.Public);
            //TryDetourFromTo(vanillaStanceDraw, newStanceDraw);

            // resets wasAutoEquipped value for picked up tools
            var vanillaTryDropSpawn = typeof(GenDrop).GetMethod("TryDropSpawn",
                BindingFlags.Static | BindingFlags.Public);
            var newTryDropSpawn = typeof(RA_GenDrop).GetMethod("TryDropSpawn",
                BindingFlags.Static | BindingFlags.Public);
            TryDetourFromTo(vanillaTryDropSpawn, newTryDropSpawn);

            // allows to add new designators to the vanilla embedded designators list
            var vanillaAllDesignators = typeof(ReverseDesignatorDatabase).GetProperty("AllDesignators",
                BindingFlags.Static | BindingFlags.Public);
            var vanillaAllDesignators_Getter = vanillaAllDesignators.GetGetMethod();
            var newAllDesignators = typeof(RA_ReverseDesignatorDatabase).GetProperty("AllDesignators",
                BindingFlags.Static | BindingFlags.Public);
            var newAllDesignators_Getter = newAllDesignators.GetGetMethod();
            TryDetourFromTo(vanillaAllDesignators_Getter, newAllDesignators_Getter);

            // allows to select which cells of the building could be used to hold ingridients
            var vanillaIngredientStackCells = typeof(Building_WorkTable).GetProperty("IngredientStackCells",
                BindingFlags.Instance | BindingFlags.Public);
            var vanillaIngredientStackCells_Getter = vanillaIngredientStackCells.GetGetMethod();
            var newIngredientStackCells = typeof(RA_Building_WorkTable).GetProperty("IngredientStackCells",
                BindingFlags.Instance | BindingFlags.Public);
            var newIngredientStackCells_Getter = newIngredientStackCells.GetGetMethod();
            TryDetourFromTo(vanillaIngredientStackCells_Getter, newIngredientStackCells_Getter);

            // change the way to determine the research bench availability
            var vanillaColonistsHaveResearchBench = typeof(ListerBuildings).GetMethod("ColonistsHaveResearchBench",
                BindingFlags.Instance | BindingFlags.Public);
            var newColonistsHaveResearchBench = typeof(RA_ListerBuildings).GetMethod("ColonistsHaveResearchBench",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaColonistsHaveResearchBench, newColonistsHaveResearchBench);

            // remove trader generation from visitor group
            var vanillaTryExecute = typeof(IncidentWorker_VisitorGroup).GetMethod("TryExecute",
                BindingFlags.Instance | BindingFlags.Public);
            var newTryExecute = typeof(RA_IncidentWorker_VisitorGroup).GetMethod("TryExecute",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaTryExecute, newTryExecute);

            //// change vanilla threat points generation
            //var vanillaDefaultParmsNow = typeof(IncidentMakerUtility).GetMethod("DefaultParmsNow", BindingFlags.Static | BindingFlags.Public);
            //var newDefaultParmsNow = typeof(RA_IncidentMakerUtility).GetMethod("DefaultParmsNow", BindingFlags.Static | BindingFlags.Public);
            //TryDetourFromTo(vanillaDefaultParmsNow, newDefaultParmsNow);

            // interrupt current job if pawn drops tool while doing it
            var vanillaNotify_Dropped = typeof(CompEquippable).GetMethod("Notify_Dropped",
                BindingFlags.Instance | BindingFlags.Public);
            var newNotify_Dropped = typeof(RA_CompEquippable).GetMethod("Notify_Dropped",
                BindingFlags.Instance | BindingFlags.Public);
            TryDetourFromTo(vanillaNotify_Dropped, newNotify_Dropped);

            // changes vanilla "wood" harvest tag to the "chop"
            var vanillaIsTree = typeof(PlantProperties).GetProperty("IsTree",
                BindingFlags.Instance | BindingFlags.Public);
            var vanillaIsTree_Getter = vanillaIsTree.GetGetMethod();
            var newIsTree = typeof(RA_PlantProperties).GetProperty("IsTree",
                BindingFlags.Instance | BindingFlags.Public);
            var newIsTree_Getter = newIsTree.GetGetMethod();
            TryDetourFromTo(vanillaIsTree_Getter, newIsTree_Getter);

            #endregion
        }
    }
}