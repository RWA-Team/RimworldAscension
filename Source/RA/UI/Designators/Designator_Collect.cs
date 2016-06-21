using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RA
{
    public abstract class Designator_Collect : Designator
    {
        public DesignationDef designationDef;
        public List<TerrainDef> allowedTerrain;

        // allows rectangular selection
        public override int DraggableDimensions => 2;
        // draws number of selected objects
        public override bool DragDrawMeasurements => true;

        public Designator_Collect()
        {
            soundDragSustain = SoundDefOf.DesignateDragStandard;
            soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            soundSucceeded = SoundDefOf.DesignateHarvest;
            activateSound = SoundDef.Named("Click");
            useMouseIcon = true;
        }

        // draws selection brackets over designated things
        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }

        // returning string text assigning false reason to AcceptanceReport
        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            if (!loc.InBounds())
            {
                return false;
            }
            if (loc.Fogged())
            {
                return false;
            }
            if (Find.DesignationManager.DesignationAt(loc, designationDef) != null)
            {
                return false;
            }
            if (!allowedTerrain.Contains(Find.TerrainGrid.TerrainAt(loc)))
            {
                return "Wrong terrain type";
            }
            return AcceptanceReport.WasAccepted;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Find.DesignationManager.AddDesignation(new Designation(c, designationDef));
        }
    }
}
