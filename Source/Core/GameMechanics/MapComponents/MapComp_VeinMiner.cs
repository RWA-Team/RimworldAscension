using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class MapComp_VeinMiner : MapComponent
    {
        public List<Designation> newDesignations = new List<Designation>();

        public override void MapComponentTick()
        {
            var designations = Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Mine);
            if (designations.Any() && GenTicks.TicksAbs % GenTicks.TickRareInterval + 47 == 0)
            {
                foreach (var designation in designations)
                {
                    var designatedMineable = designation.target.Thing;
                    var cellsAround = GenAdj.CellsAdjacent8Way(designation.target.Thing);
                    foreach (var cell in cellsAround)
                    {
                        var adjucentMineable = MineUtility.MineableInCell(cell);
                        if (adjucentMineable.def == designatedMineable.def)
                        {
                            AutoUtility.TryAutoDesignate(new Designator_Mine(), "Mine", adjucentMineable);
                        }
                    }
                }
            }
        }
    }
}
