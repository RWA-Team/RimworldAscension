using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RA
{
    public static class FindUtil
    {
        public static IEnumerable<IntVec3> SquareAreaAround(IntVec3 root, int range, IEnumerable<IntVec3> exceptedArea = null)
        {
            var currentCell = root;

            for (var x = root.x - range; x < root.x + range + 1; x++)
            {
                currentCell.x = x;
                for (var z = root.z - range; z < root.z + range + 1; z++)
                {
                    currentCell.z = z;
                    if (!exceptedArea?.Contains(currentCell) ?? false)
                        yield return currentCell;
                }
            }
        }
    }
}