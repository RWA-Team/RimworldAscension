using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RA
{
    public static class ResearchMods
    {
        #region NEOLITHIC

        public static void BasicFarming()
        {
            AddDesignator(new Designator_ZoneAdd_Growing(), "Zone");
        }

        public static void ExamineFauna()
        {
            AddDesignator(new Designator_Hunt(), "Orders");
        }

        public static void AnimalHusbandry()
        {
            AddDesignator(new Designator_Tame(), "Orders");
            AddDesignator(new Designator_Slaughter(), "Orders");
        }

        public static void WoodWorking()
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

        public static void StoneWorking()
        {
            TryAllowToBuild("CraftsmanBench");
            TryAllowToBuild("WarriorsBench", true, "WoodWorking");
            TryAllowToBuild("HuntersBench", true, "WoodWorking");

            TryAllowToCraft("Make_Primitive_Handaxe", false, "WoodWorking");
            TryAllowToCraft("Make_Primitive_Hammer", false, "WoodWorking");

            TryAllowToCraft("Make_Shiv", true, "WoodWorking");
            AddResourceTypeToRecipe("Make_Shiv", "StoneBlocks");
            TryAllowToCraft("Make_Spear", true, "WoodWorking");
            AddResourceTypeToRecipe("Make_Spear", "StoneBlocks");
            TryAllowToCraft("Make_Club", true, "WoodWorking");
            AddResourceTypeToRecipe("Make_Club", "StoneBlocks");

            TryAllowToCraft("Make_Tomahawk", true, "WoodWorking");
        }

        public static void BoneWorking()
        {
            TryAllowToBuild("CraftsmanBench");
            TryAllowToBuild("WarriorsBench", true, "WoodWorking");
            TryAllowToBuild("HuntersBench", true, "WoodWorking");

            TryAllowToCraft("Make_Shiv", true, "WoodWorking");
            AddResourceTypeToRecipe("Make_Shiv", "Bone");
            TryAllowToCraft("Make_Spear", true, "WoodWorking");
            AddResourceTypeToRecipe("Make_Spear", "Bone");
            TryAllowToCraft("Make_Club", true, "WoodWorking");
            AddResourceTypeToRecipe("Make_Club", "Bone");
            TryAllowToCraft("Make_Buckler", true, "WoodWorking");
            AddResourceTypeToRecipe("Make_Buckler", "Bone");

            TryAllowToCraft("Make_Composite_Bow", true, "WoodWorking");
            TryAllowToCraft("Make_Thrumbo_Bow", true, "WoodWorking");
            TryAllowToCraft("Make_Pilum", true, "WoodWorking");
        }

        public static void LeatherWorking()
        {
            TryAllowToBuild("CraftsmanBench");
            TryAllowToBuild("HuntersBench");

            TryAllowToCraft("Make_Buckler", true, "WoodWorking");
            AddResourceTypeToRecipe("Make_Buckler", "Leathers");

            TryAllowToCraft("Make_Sling");
            
            TryAllowToCraft("Make_Backpack");
            TryAllowToCraft("Make_Toolbelt");

            TryAllowToCraft("Make_Loincloth");
            TryAllowToCraft("Make_Tribalwear");
            TryAllowToCraft("Make_Hat");
        }

        #endregion

        /*
        public static void Medieval()
        {
            FactionDef colony = DefDatabase<FactionDef>.GetNamed("Colony", true);
            colony.techLevel = TechLevel.Medieval;
        }
        */

        //  ********************************************************\


        #region METHODS

        // adds building to the architect menu if all other research prerequisites are met
        public static void TryAllowToBuild(string benchDefName, bool allowIfAll = true, params string[] otherResearchPrerequisite_defNames)
        {
            if (allowIfAll)
            {
                // check if all prerequisites are met
                foreach (string researchName in otherResearchPrerequisite_defNames)
                {
                    if (!Find.ResearchManager.IsFinished(ResearchProjectDef.Named(researchName)))
                    {
                        // some prerequisites not met, return
                        return;
                    }
                }

                ThingDef.Named(benchDefName).researchPrerequisite = null;
            }
            else
            {
                // check if any of prerequisites are met
                foreach (string researchName in otherResearchPrerequisite_defNames)
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
                foreach (string researchName in otherResearchPrerequisite_defNames)
                {
                    if (!Find.ResearchManager.IsFinished(ResearchProjectDef.Named(researchName)))
                    {
                        // some prerequisites not met, return
                        return;
                    }
                }

                DefDatabase<RecipeDef>.GetNamed(recipeDefName).researchPrerequisite = null;
            }
            else
            {
                // check if any of prerequisites are met
                foreach (string researchName in otherResearchPrerequisite_defNames)
                {
                    if (Find.ResearchManager.IsFinished(ResearchProjectDef.Named(researchName)))
                    {
                        DefDatabase<RecipeDef>.GetNamed(recipeDefName).researchPrerequisite = null;
                    }
                }
            }
        }

        // adds resource type to the recipe ingridients filter
        public static void AddResourceTypeToRecipe(string recipeDefName, string resourceTypeName)
        {
            // check if resource type is category
            if (DefDatabase<ThingCategoryDef>.GetNamedSilentFail(resourceTypeName) != null)
                DefDatabase<RecipeDef>.GetNamedSilentFail(recipeDefName).fixedIngredientFilter.SetAllow(ThingCategoryDef.Named(resourceTypeName), true);

            // check if resource type is thing
            if (DefDatabase<ThingDef>.GetNamedSilentFail(resourceTypeName) != null)
                DefDatabase<RecipeDef>.GetNamedSilentFail(recipeDefName).fixedIngredientFilter.SetAllow(ThingDef.Named(resourceTypeName), true);
        }

        // adds designato to the dame
        public static void AddDesignator(Designator designator, string designationCategoryDefName)
        {
            DesignationCategoryDef category = DefDatabase<DesignationCategoryDef>.GetNamed(designationCategoryDefName);
            category.resolvedDesignators.Add(designator);
        }

        #endregion
    }
}