
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class ResearchMods
    {
        #region NEOLITHIC
        // +
        public static void BasicFarming()
        {
            AddDesignator(new Designator_ZoneAdd_Growing(), "Zone");
        }
        // +
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
        // +
        public static void AnimalHusbandry()
        {
            AddDesignator(new Designator_Tame(), "Orders");
        }

        public static void Carpentry()
        {
            TryAllowToBuild("CraftsmanBench");
            TryAllowToBuild("HuntersBench");
            TryAllowToBuild("WarriorsBench", false, "StoneWorking", "BoneWorking");

            TryAllowToCraft("Make_Primitive_Handaxe", true, "StoneWorking");
            TryAllowToCraft("Make_Primitive_Hammer", true, "StoneWorking");

            TryAllowToCraft("Make_Shiv", false, "StoneWorking", "BoneWorking");
            TryAllowToCraft("Make_Spear", false, "StoneWorking", "BoneWorking");
            TryAllowToCraft("Make_Club", false, "StoneWorking", "BoneWorking");

            TryAllowToCraft("Make_Bow");
            TryAllowToCraft("Make_Composite_Bow", true, "BoneWorking");
            TryAllowToCraft("Make_Thrumbo_Bow", true, "BoneWorking");
            TryAllowToCraft("Make_Pilum", true, "BoneWorking");
            TryAllowToCraft("Make_Tomahawk", true, "StoneWorking");
        }

        public static void Masonry()
        {
            TryAllowToBuild("CraftsmanBench");
            TryAllowToBuild("WarriorsBench", true, "WoodWorking");
            TryAllowToBuild("HuntersBench", true, "WoodWorking");

            TryAllowToCraft("Make_Primitive_Handaxe", false, "WoodWorking");
            TryAllowToCraft("Make_Primitive_Hammer", false, "WoodWorking");

            TryAllowToCraft("Make_Shiv", true, "WoodWorking");
            TryAllowToCraft("Make_Spear", true, "WoodWorking");
            TryAllowToCraft("Make_Club", true, "WoodWorking");

            TryAllowToCraft("Make_Tomahawk", true, "WoodWorking");
        }

        public static void Tannery()
        {
            TryAllowToBuild("CraftsmanBench");
            TryAllowToBuild("HuntersBench");

            TryAllowToCraft("Make_Buckler", true, "WoodWorking");

            TryAllowToCraft("Make_Sling");

            TryAllowToCraft("Make_Backpack");
            TryAllowToCraft("Make_Toolbelt");

            TryAllowToCraft("Make_Loincloth");
            TryAllowToCraft("Make_Tribalwear");
            TryAllowToCraft("Make_Hat");
        }

        public static void BoneCarving()
        {
            TryAllowToBuild("CraftsmanBench");
            TryAllowToBuild("WarriorsBench", true, "WoodWorking");
            TryAllowToBuild("HuntersBench", true, "WoodWorking");

            TryAllowToCraft("Make_Shiv", true, "WoodWorking");
            TryAllowToCraft("Make_Spear", true, "WoodWorking");
            TryAllowToCraft("Make_Club", true, "WoodWorking");
            TryAllowToCraft("Make_Buckler", true, "WoodWorking");

            TryAllowToCraft("Make_Composite_Bow", true, "WoodWorking");
            TryAllowToCraft("Make_Thrumbo_Bow", true, "WoodWorking");
            TryAllowToCraft("Make_Pilum", true, "WoodWorking");
        }

        #endregion

        #region UTILITY

        // adds building to the architect menu if all other research prerequisites are met
        public static void TryAllowToBuild(string benchDefName, bool allowIfAll = true, params string[] otherResearchPrerequisite_defNames)
        {
            if (allowIfAll)
            {
                // check if all prerequisites are met
                if (otherResearchPrerequisite_defNames.Any(researchName => !Find.ResearchManager.IsFinished(ResearchProjectDef.Named(researchName))))
                {
                    return;
                }

                ThingDef.Named(benchDefName).researchPrerequisite = null;
            }
            else
            {
                // check if any of prerequisites are met
                foreach (var researchName in otherResearchPrerequisite_defNames)
                {
                    if (Find.ResearchManager.IsFinished(ResearchProjectDef.Named(researchName)))
                    {
                        ThingDef.Named(benchDefName).researchPrerequisite = null;
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
                // if type is category
                if (thingDef == null)
                {
                    if (!recipeDef.fixedIngredientFilter.exceptedCategories.Contains(resourceTypeName))
                    {
                        recipeDef.fixedIngredientFilter.exceptedCategories.Remove(resourceTypeName);
                        recipeDef.fixedIngredientFilter.SetAllow(ThingCategoryDef.Named(resourceTypeName), true);
                    }
                }
                // if it's a thingDef
                else
                {
                    if (!recipeDef.fixedIngredientFilter.exceptedThingDefs.Contains(thingDef))
                    {
                        recipeDef.fixedIngredientFilter.exceptedThingDefs.Remove(thingDef);
                        recipeDef.fixedIngredientFilter.SetAllow(thingDef, true);
                    }
                }
            }
        }

        #endregion
    }
}