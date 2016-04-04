using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public abstract class RA_Designator_Place : Designator_Place
    {
        /// added rectangular field edges support for trading post
        /// added Graphic_StuffBased support
        public override void SelectedUpdate()
        {
            GenDraw.DrawNoBuildEdgeLines();
            if (!RA_ArchitectCategoryTab.InfoRect.Contains(GenUI.AbsMousePosition()))
            {
                var intVec = Gen.MouseCell();
                if (PlacingDef is TerrainDef)
                {
                    GenUI.RenderMouseoverBracket();
                    return;
                }

                var ghostCol = CanDesignateCell(intVec).Accepted ? new Color(0.5f, 1f, 0.6f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);

                // special Graphic_StuffBased implementation
                var baseGraphic = PlacingDef.graphic as Graphic_StuffBased;

                GhostDrawer.DrawGhostThing(Gen.MouseCell(), placingRot, (ThingDef)PlacingDef, baseGraphic?.categorizedGraphics[baseGraphic.currentCategory], ghostCol, AltitudeLayer.Blueprint);

                if (CanDesignateCell(intVec).Accepted && PlacingDef.specialDisplayRadius > 0.01f)
                {
                    if (PlacingDef.defName.Contains("TradingPost"))
                    {
                        GenDraw.DrawFieldEdges(Building_TradingPost.TradeableCells(Gen.MouseCell(), (int)PlacingDef.specialDisplayRadius + 1).ToList());
                    }
                    else
                        GenDraw.DrawRadiusRing(Gen.MouseCell(), PlacingDef.specialDisplayRadius);
                }
                GenDraw.DrawInteractionCell((ThingDef)PlacingDef, intVec, placingRot);
            }
        }
    }
}
