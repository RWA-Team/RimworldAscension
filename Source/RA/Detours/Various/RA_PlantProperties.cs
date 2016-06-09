using RimWorld;

namespace RA
{
    // changes vanilla "wood" harvest tag to the "chop"
    public class RA_PlantProperties : PlantProperties
    {
        public new bool IsTree => harvestTag == "Chop";
    }
}
