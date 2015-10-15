using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RimworldAscension
{
    public static class ResearchMods
    {
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
            ThingDef craftBench = DefDatabase<ThingDef>.GetNamed("CraftingTable_Neolithic", true);
            craftBench.researchPrerequisite = null;

            if (Find.ResearchManager.IsFinished(DefDatabase<ResearchProjectDef>.GetNamed("StoneWorking", true)))
            {
                // NOTE: add neolithic tools recipe

                ThingDef meleeBench = DefDatabase<ThingDef>.GetNamed("MeleeWeaponTable_Neolithic", true);
                meleeBench.researchPrerequisite = null;
            }
        }

        public static void StoneWorking()
        {
            ThingDef craftBench = DefDatabase<ThingDef>.GetNamed("CraftingTable_Neolithic", true);
            craftBench.researchPrerequisite = null;

            if (Find.ResearchManager.IsFinished(DefDatabase<ResearchProjectDef>.GetNamed("WoodWorking", true)))
            {
                // NOTE: add neolithic tools recipe

                ThingDef meleeBench = DefDatabase<ThingDef>.GetNamed("MeleeWeaponTable_Neolithic", true);
                meleeBench.researchPrerequisite = null;
            }
        }

        /*
        public static void Medieval()
        {
            FactionDef colony = DefDatabase<FactionDef>.GetNamed("Colony", true);
            colony.techLevel = TechLevel.Medieval;
        }
        */

        //  ********************************************************\


        public static void AddDesignator(Designator designator, string designationCategoryDefName)
        {
            DesignationCategoryDef category = DefDatabase<DesignationCategoryDef>.GetNamed(designationCategoryDefName, true);
            category.resolvedDesignators.Add(designator);
        }
    }
}