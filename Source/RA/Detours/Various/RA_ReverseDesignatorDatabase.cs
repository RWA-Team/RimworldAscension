using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public static class RA_ReverseDesignatorDatabase
    {
        public static List<Designator> desList;

        // allows to add new designators to the vanilla embedded designators list
        public static List<Designator> AllDesignators => desList ?? (desList = new List<Designator>
        {
            new Designator_Cancel(),
            new Designator_Claim(),
            new Designator_Deconstruct(),
            new Designator_Uninstall(),
            new Designator_Haul(),
            new Designator_Hunt(),
            new Designator_Slaughter(),
            new Designator_Tame(),
            new Designator_PlantsCut(),
            new Designator_PlantsHarvest(),
            // special RA designator for wood
            new Designator_ChopWood(),
            new Designator_Mine(),
            new Designator_Strip(),
            new Designator_RearmTrap(),
            new Designator_Open()
        });
    }
}
