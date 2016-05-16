
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
            TryAllowWorktables();
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
            foreach (var pawnDef in DefDatabase<ThingDef>.AllDefs.Where(pawnDef => pawnDef.race?.IsFlesh == true))
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
            // craftsman table
            TryAllowToCraft("MakeFigurine", "WoodLog");

            // hunter table
            TryAllowToCraft("MakeTrapDeadfall", "WoodLog");
        }

        public static void Masonry()
        {
            AddDesignator(new Designator_SmoothFloor(), "Floors");

            // craftsman table
            TryAllowToCraft("MakeFigurine", "StoneCobbles");

            // hunter table
            TryAllowToCraft("MakeTrapDeadfall", "StoneCobbles");
            TryAllowToCraft("MakeTomahawk", "StoneCobbles", true, "Carpentry");

            // warrior table
            TryAllowToCraft("MakeShiv", "StoneCobbles");
            TryAllowToCraft("MakeSpearNeolithic", "StoneCobbles", true, "Carpentry");
            TryAllowToCraft("MakeAxeNeolithic", "StoneCobbles", true, "Carpentry");
            TryAllowToCraft("MakeHammerNeolithic", "StoneCobbles", true, "Carpentry");
        }

        public static void BoneCarving()
        {
            // craftsman table
            TryAllowToCraft("MakeFigurine", "Bone");

            // hunter table
            TryAllowToCraft("MakeTrapDeadfall", "Bone");
            TryAllowToCraft("MakePilum", "Bone", true, "Carpentry");
            TryAllowToCraft("MakeBowComposite", "Bone", true, "Carpentry");
            TryAllowToCraft("MakeBowThrumbo", "Bone", true, "Carpentry");

            // warrior table
            TryAllowToCraft("MakeShiv", "Bone");
            TryAllowToCraft("MakeSpearNeolithic", "Bone", true, "Carpentry");
            TryAllowToCraft("MakeAxeNeolithic", "Bone", true, "Carpentry");
            TryAllowToCraft("MakeHammerNeolithic", "Bone", true, "Carpentry");
        }

        #endregion

        #region UTILITY

        // adds building to the architect menu if all other research prerequisites are met
        public static void TryAllowWorktables()
        {
            foreach (var tableDef in DefDatabase<ThingDef>.AllDefs.Where(def => !def.recipes.NullOrEmpty()))
            {
                if (!tableDef.researchPrerequisites.NullOrEmpty())
                {
                    if (tableDef.recipes.Any(recipeDef => recipeDef.AvailableNow))
                    {
                        tableDef.researchPrerequisites = null;
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

        // adds recipe to the bench if all or any other research prerequisites are met
        public static void TryAllowToCraft(string recipeDefName, string resourceTypeName = default(string),
            bool allowIfAll = true,
            params string[] otherResearchPrerequisite_defNames)
        {
            if (allowIfAll)
            {
                // check if all prerequisites are met
                if (
                    otherResearchPrerequisite_defNames.Any(
                        researchName => !Find.ResearchManager.IsFinished(ResearchProjectDef.Named(researchName))))
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

            if (resourceTypeName != default(string))
                AllowResourceTypeForRecipe(recipeDefName, resourceTypeName);
        }

        // adds resource type to the allowed list in recipe ingridients filter (ignores excepted list)
        public static void AllowResourceTypeForRecipe(string recipeDefName, string resourceTypeName)
        {
            var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(resourceTypeName);
            // if it's a thingDef
            if (thingDef != null)
                DefDatabase<RecipeDef>.GetNamedSilentFail(recipeDefName).fixedIngredientFilter.SetAllow(thingDef, true);
            // if type is category
            else
                DefDatabase<RecipeDef>.GetNamedSilentFail(recipeDefName)
                    .fixedIngredientFilter.SetAllow(ThingCategoryDef.Named(resourceTypeName), true);
        }

        #endregion
    }
}