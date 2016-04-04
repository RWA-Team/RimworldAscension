
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class ResearchMods
    {
        public static void StartPack()
        {
            CheckWorkTablesAllowance();
        }

        #region NEOLITHIC

        public static void BasicFarming()
        {
            AddDesignator(new Designator_ZoneAdd_Growing(), "Zone");
        }

        public static void Butchering()
        {
            AddDesignator(new Designator_Slaughter(), "Orders");

            // all meat based pawns
            foreach (var pawnDef in DefDatabase<ThingDef>.AllDefs.Where(pawnDef => pawnDef.race?.isFlesh == true))
            {
                if (pawnDef.butcherProducts.NullOrEmpty())
                    pawnDef.butcherProducts = new List<ThingCount>();
                // each pawn generate it's (body size)x5 bone amount
                pawnDef.butcherProducts.Add(new ThingCount(ThingDef.Named("Bone"), (int) pawnDef.race.baseBodySize*5));
            }
        }

        public static void AnimalHusbandry()
        {
            AddDesignator(new Designator_Tame(), "Orders");
        }

        public static void Carpentry()
        {
            AllowResourceTypeForAllRecipes("WoodLog");

            // builder table
            TryAllowToCraft("MakeStoolPrimitive");
            TryAllowToCraft("MakeTableShort");
            TryAllowToCraft("MakeTableLong");
            TryAllowToCraft("MakeTotem");

            // craftsman table
            TryAllowToCraft("MakeFigurine");

            // hunter table
            TryAllowToCraft("MakeTrapDeadfall");
        }

        public static void Masonry()
        {
            AddDesignator(new Designator_SmoothFloor(), "Floor");
            AllowResourceTypeForAllRecipes("StoneBlocks");

            // builder table
            TryAllowToCraft("MakeStoolPrimitive");
            TryAllowToCraft("MakeTableShort");
            TryAllowToCraft("MakeTableLong");
            TryAllowToCraft("MakeTotem");

            // craftsman table
            TryAllowToCraft("MakeFigurine");

            // hunter table
            TryAllowToCraft("MakeTrapDeadfall");
            TryAllowToCraft("MakeTomahawk", true, "Carpentry");

            // warrior table
            TryAllowToCraft("MakeShiv");
            TryAllowToCraft("MakeSpearPrimitive", true, "Carpentry");
            TryAllowToCraft("MakeAxePrimitive", true, "Carpentry");
            TryAllowToCraft("MakeHammerPrimitive", true, "Carpentry");
        }

        public static void BoneCarving()
        {
            AllowResourceTypeForAllRecipes("Bone");

            // craftsman table
            TryAllowToCraft("MakeFigurine");

            // hunter table
            TryAllowToCraft("MakeTrapDeadfall");
            TryAllowToCraft("MakePilum", true, "Carpentry");
            TryAllowToCraft("MakeBowComposite", true, "Carpentry");
            TryAllowToCraft("MakeBowThrumbo", true, "Carpentry");

            // warrior table
            TryAllowToCraft("MakeShiv");
            TryAllowToCraft("MakeSpearPrimitive", true, "Carpentry");
            TryAllowToCraft("MakeAxePrimitive", true, "Carpentry");
            TryAllowToCraft("MakeHammerPrimitive", true, "Carpentry");
        }

        public static void Tannery()
        {
            TryAllowToCraft("MakeBedHide", true, "Carpentry");
        }

        #endregion

        #region UTILITY

        // adds building to the architect menu if all other research prerequisites are met
        public static void CheckWorkTablesAllowance()
        {
            foreach (var tableDef in DefDatabase<ThingDef>.AllDefs.Where(def => !def.recipes.NullOrEmpty()))
            {
                if (tableDef.researchPrerequisite != null)
                {
                    if (tableDef.recipes.Any(recipeDef => recipeDef.AvailableNow))
                    {
                        tableDef.researchPrerequisite = null;
                    }
                }
            }
        }

        // adds recipe to the bench if all or any other research prerequisites are met
        public static void TryAllowToCraft(string recipeDefName, bool allowIfAll = true, params string[] otherResearchPrerequisite_defNames)
        {
            if (allowIfAll)
            {
                // check if all prerequisites are met
                if (otherResearchPrerequisite_defNames.Any(researchName => !Find.ResearchManager.IsFinished(ResearchProjectDef.Named(researchName))))
                {
                    return;
                }

                DefDatabase<RecipeDef>.GetNamed(recipeDefName).researchPrerequisite = null;
            }
            else
            {
                // check if any of prerequisites are met
                foreach (var researchName in otherResearchPrerequisite_defNames)
                {
                    if (Find.ResearchManager.IsFinished(ResearchProjectDef.Named(researchName)))
                    {
                        DefDatabase<RecipeDef>.GetNamed(recipeDefName).researchPrerequisite = null;
                    }
                }
            }
        }

        // adds designator to the game
        public static void AddDesignator(Designator designator, string designationCategoryDefName)
        {
            var category = DefDatabase<DesignationCategoryDef>.GetNamed(designationCategoryDefName);
            category.resolvedDesignators.Add(designator);
        }

        // adds resource type to the allowed list in recipe ingridients filter (removes it from excepted list)
        public static void AllowResourceTypeForAllRecipes(string resourceTypeName)
        {
            var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(resourceTypeName);

            foreach (var recipeDef in DefDatabase<RecipeDef>.AllDefs)
            {
                // if it's a thingDef
                if (thingDef != null)
                {
                    if (!recipeDef.fixedIngredientFilter?.exceptedThingDefs?.Contains(thingDef) ?? false)
                    {
                        recipeDef.fixedIngredientFilter.exceptedThingDefs.Remove(thingDef);
                        //recipeDef.fixedIngredientFilter.SetAllow(thingDef, true);
                        recipeDef.fixedIngredientFilter.thingDefs.Add(thingDef);
                    }
                }
                // if type is category
                else
                {
                    if (!recipeDef.fixedIngredientFilter?.exceptedCategories?.Contains(resourceTypeName) ?? false)
                    {
                        recipeDef.fixedIngredientFilter.exceptedCategories.Remove(resourceTypeName);
                        //recipeDef.fixedIngredientFilter.SetAllow(ThingCategoryDef.Named(resourceTypeName), true);
                        recipeDef.fixedIngredientFilter.categories.Add(resourceTypeName);
                    }
                }
            }
        }

        #endregion
    }
}