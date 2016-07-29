using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_Building_WorkTable : Building_WorkTable
    {
        // defining required comps here for convinience
        public CompWorktableExtended compWorktableExtended;
        public List<IntVec3> allowedCells = new List<IntVec3>();

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            // required comp initialize
            compWorktableExtended = this.TryGetComp<CompWorktableExtended>();

            // allowed cells changed
            if (!compWorktableExtended?.Properties.ingridientCells.NullOrEmpty() ?? false)
            {
                foreach (var cell in compWorktableExtended.Properties.ingridientCells)
                {
                    if (IngredientStackCells.Contains(Position + cell))
                        allowedCells.Add(Position + cell);
                }
            }
        }

        //public override void Tick()
        //{
        //    base.SpawnSetup();

        //    // required comp initialize
        //    compWorktableExtended = this.TryGetComp<CompWorktableExtended>();

        //    // allowed cells changed
        //    allowedCells = new List<IntVec3>();
        //    foreach (var cell in compWorktableExtended.Props.ingridientCells)
        //    {
        //        if (IngredientStackCells.Contains(Position + cell))
        //            allowedCells.Add(Position + cell);
        //    }
        //}

        // allows to select which cells of the building could be used to hold ingridients
        public new IEnumerable<IntVec3> IngredientStackCells
            => (this.TryGetComp<CompWorktableExtended>()?.Properties.ingridientCells.NullOrEmpty() ?? false)
                ? allowedCells
                : GenAdj.CellsOccupiedBy(this);
    }
}
