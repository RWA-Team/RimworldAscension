using System.Linq;
using UnityEngine;
using Verse;

namespace RA
{
    public class PlaceWorker_TradeCenterArea : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            GenDraw.DrawFieldEdges(
                FindUtil.SquareAreaAround(center, Mathf.RoundToInt(def.specialDisplayRadius))
                    .Where(cell => cell.Walkable() && cell.InBounds())
                    .ToList());
        }
    }
}