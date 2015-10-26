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
            ThingDef craftBench = ThingDef.Named("CraftingTable_Neolithic");
            craftBench.researchPrerequisite = null;

            if (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("StoneWorking")))
            {
                // NOTE: add neolithic tools recipe

                ThingDef meleeBench = ThingDef.Named("MeleeWeaponTable_Neolithic");
                meleeBench.researchPrerequisite = null;
            }
        }

        public static void StoneWorking()
        {
            ThingDef craftBench = ThingDef.Named("CraftingTable_Neolithic");
            craftBench.researchPrerequisite = null;

            if (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("WoodWorking")))
            {
                // NOTE: add neolithic tools recipe

                ThingDef meleeBench = ThingDef.Named("MeleeWeaponTable_Neolithic");
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