using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI.Group;
using static System.String;

namespace RA
{
    [StaticConstructorOnStartup]
    public class Initializer
    {
        public static BindingFlags universalFlags = GenGeneric.BindingFlagsAll;

        public static List<string> sourceMethods = new List<string>();
        public static List<string> destMethods = new List<string>();

        static Initializer()
        {
            DoDetours();

            RA_DefGenerator.GenerateDefs();
            DesignatorUtil.CombineBuildDesignators();
        }

        public static void DetourMethod(string className, string methodName)
        {
            var vanillaMethodInfo = GenTypes.GetTypeInAnyAssembly(className)?.GetMethod(methodName, universalFlags);
            var raMethodInfo = GenTypes.GetTypeInAnyAssembly("RA.RA_" + className)?.GetMethod(methodName, universalFlags);

            if (vanillaMethodInfo == null || raMethodInfo== null || !TryDetourFromTo(vanillaMethodInfo, raMethodInfo))
                Log.Warning(Format("Failed to detour {0} in {1}.", methodName, className));
        }

        public static void DetourProperty(string className, string propertyName)
        {
            var vanillaPropertyInfo = GenTypes.GetTypeInAnyAssembly(className)?.GetProperty(propertyName, universalFlags);
            var vanillaPropertyInfoGetter = vanillaPropertyInfo?.GetGetMethod();
            var raPropertyInfo = GenTypes.GetTypeInAnyAssembly("RA.RA_" + className)?
                .GetProperty(propertyName, universalFlags);
            var raPropertyInfoGetter = raPropertyInfo?.GetGetMethod();

            if (vanillaPropertyInfoGetter == null || raPropertyInfoGetter == null || !TryDetourFromTo(vanillaPropertyInfoGetter, raPropertyInfoGetter))
                Log.Warning(Format("Failed to detour {0} in {1}.", propertyName, className));
        }

        public static object GetHiddenValue(Type type, object instance, string fieldName, FieldInfo info)
        {
            if (info == null)
            {
                info = type.GetField(fieldName, universalFlags);
            }

            return info?.GetValue(instance);
        }

        public static void SetHiddenValue(object value, Type type, object instance, string fieldName, FieldInfo info)
        {
            if (info == null)
            {
                info = type.GetField(fieldName, universalFlags);
            }

            info?.SetValue(instance, value);
        }

        // performs detours, spits out basic logs and warns if a method is sourceMethods multiple times.
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

            var vanillaNotify_PawnTookDamage = typeof(Lord).GetMethod("Notify_PawnTookDamage", universalFlags);
            var newNotify_PawnTookDamage = typeof(RA_Lord).GetMethod("Notify_PawnTookDamage", universalFlags);
            TryDetourFromTo(vanillaNotify_PawnTookDamage, newNotify_PawnTookDamage);

            #endregion

            #region COMBAT
            
            // flat damage reduction armor system and armor penetration
            DetourMethod("DamageWorker_AddInjury", "ApplyDamagePartial");
            // flat damage reduction armor system and armor penetration
            DetourMethod("DamageWorker_AddInjury", "CheckDuplicateDamageToOuterParts");
            // flat damage reduction armor system and armor penetration
            DetourMethod("DamageWorker_AddInjury", "CheckPropagateDamageToInnerSolidParts");

            #endregion

            #region CONTAINERS

            // modifies rotting speed based on container stats
            DetourMethod("CompRottable", "CompTickRare");
            // allows to place items to container stacks
            DetourMethod("GenPlace", "TryPlaceDirect");
            // allows to haul items to container stacks
            DetourMethod("HaulAIUtility", "HaulMaxNumToCellJob");
            // allows to pass checks for item stacking in cells with containers
            DetourMethod("StoreUtility", "NoStorageBlockersIn");

            #endregion

            #region DESIGNATORS

            // changes initial log window size
            DetourMethod("Designator_Build", "ProcessInput");
            // added special case for minified things
            DetourMethod("Designator_Build", "DrawPanelReadout");
            // draws texture and build cost while building
            // modified call for InfoRect
            // added special case for minified things
            DetourMethod("Designator_Build", "DrawMouseAttachments");
            // make vanilla build gizmo open building groups
            DetourMethod("Designator_Build", "GizmoOnGUI");
            // make plant cut designator work on plants with "Cut" harvest tag only
            DetourMethod("Designator_Plants", "CanDesignateThing");
            // make plant harvest designator work on plants with "Harvest" harvest tag only
            DetourMethod("Designator_PlantsHarvest", "CanDesignateThing");
            // make mine designator work after special research is made
            DetourMethod("Designator_Mine", "CanDesignateThing");

            #endregion

            #region JOBS

            // fixes decreased carry capacity
            DetourMethod("JobDriver_DoBill", "JumpToCollectNextIntoHandsForBill");
            // changed how prodcut stuff type is determined and made this toil assign production cost for the Thing to the CompCraftedValue
            DetourMethod("Toils_Recipe", "FinishRecipeAndStartStoringProduct");
            // added support for fuel burners
            DetourMethod("Toils_Recipe", "DoRecipeWork");
            // skip message prompt for research recipes
            DetourMethod("Bill_Production", "Notify_IterationCompleted");

            #endregion

            #region TRADE

            // made trade system accept items for trade in trade zones around trade centers
            DetourProperty("Pawn", "ColonyThingsWillingToBuy");
            // made trade system offer items for trade from tradeStock in current trade center
            DetourProperty("Pawn", "Goods");
            // resolve trade by assigning hauling jobs
            DetourMethod("Tradeable", "ResolveTrade");
            // resolve trade for pawns
            DetourMethod("Tradeable_Pawn", "ResolveTrade");
            // make trade sessions individual per trade center
            DetourMethod("TradeSession", "SetupWith");
            // assign Pawn_TraderTracker based on pawn caravan role, not mindState.wantsToTradeWithColony
            DetourMethod("PawnComponentsUtility", "AddAndRemoveDynamicComponents");

            #endregion

            #region UI

            // changes initial log window size
            DetourMethod("EditWindow_Log", "PostOpen");
            // added skipping of steam requirement prompt
            DetourMethod("UIRoot_Entry", "Init");
            // detour RimWorld.MainMenuDrawer.MainMenuOnGUI
            DetourMethod("MainMenuDrawer", "MainMenuOnGUI");
            // detour RimWorld.UI_BackgroundMain.BackgroundOnGUI
            DetourMethod("UI_BackgroundMain", "BackgroundOnGUI");
            // draws hands on equipment, if corresponding Comp is specified
            DetourMethod("PawnRenderer", "DrawEquipment");
            //changed inner graphic extraction for minified things
            DetourMethod("GraphicUtility", "ExtractInnerGraphicFor");
            // changed text align to middle center
            DetourMethod("Widgets", "ButtonTextSubtle");
            // make ProgressBar effecter deteriorate used tools
            DetourMethod("SubEffecter_ProgressBar", "SubEffectTick");
            // resolves reference to MainTabWindow_Architect
            DetourProperty("ArchitectCategoryTab", "InfoRect");

            #endregion

            #region NPC

            //// allow AffectGoodwillWith with hidden factions; make factions hostile when relations are negative
            //DetourMethod("Faction", "AffectGoodwillWith");
            //// allow Notify_MemberCaptured with hidden factions
            //DetourMethod("Faction", "Notify_MemberCaptured");
            //// allow Notify_MemberTookDamage with hidden factions
            //DetourMethod("Faction", "Notify_MemberTookDamage");
            //// allow Notify_FactionTick with hidden factions
            //// detour RimWorld.Faction.Notify_FactionTick
            //DetourMethod("Faction", "Notify_FactionTick");
            //// allow SetHostileTo with hidden factions
            //DetourMethod("Faction", "SetHostileTo");
            //// removes hidden factions restriction from GenerateFactionsIntoCurrentWorld
            //DetourMethod("FactionGenerator", "GenerateFactionsIntoCurrentWorld");
            //// detour Verse.Pawn.ExitMap
            //DetourMethod("Pawn", "ExitMap");

            #endregion

            #region VARIOUS

            // removed selecting of items in containers
            DetourMethod("ThingSelectionUtility", "SelectableNow");
            // changed butcher yields
            DetourMethod("Pawn", "ButcherProducts");
            // special spawns for skyfaller impact explosion (otherwise things are damaged by eplosion even if spawned after that, but without delay)
            DetourMethod("Explosion", "TrySpawnExplosionThing");
            //// CR aim pie
            //DetourMethod("Stance_Warmup", "StanceDraw");
            // resets wasAutoEquipped value for picked up tools
            DetourMethod("GenDrop", "TryDropSpawn");
            // allows to add new designators to the vanilla embedded designators list
            DetourProperty("ReverseDesignatorDatabase", "AllDesignators");
            // allows to select which cells of the building could be used to hold ingridients
            DetourProperty("Building_WorkTable", "IngredientStackCells");
            // change the way to determine the research bench availability
            DetourMethod("ListerBuildings", "ColonistsHaveResearchBench");
            // remove trader generation from visitor group
            DetourMethod("IncidentWorker_VisitorGroup", "TryExecute");
            //// change vanilla threat points generation
            //DetourMethod("IncidentMakerUtility", "DefaultParmsNow");
            // interrupt current job if pawn drops tool while doing it
            DetourMethod("CompEquippable", "Notify_Dropped");
            // changes vanilla "wood" harvest tag to the "chop"
            DetourProperty("PlantProperties", "IsTree");
            // delay GameEndTick first call
            DetourMethod("GameEnder", "GameEndTick");
            // delay CheckGameOver first call
            DetourMethod("GameEnder", "CheckGameOver");

            #endregion
        }
    }
}