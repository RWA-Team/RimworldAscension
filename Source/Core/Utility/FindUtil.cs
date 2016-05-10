using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RA
{
    public static class FindUtil
    {
        public static IEnumerable<IntVec3> SquareAreaAround(IntVec3 root, int range)
        {
            // half width of the rectangle, determined by <specialDisplayRadius> in building def
            var cell = root;

            for (var x = root.x - range; x < root.x + range + 1; x++)
            {
                cell.x = x;
                for (var z = root.z - range; z < root.z + range + 1; z++)
                {
                    cell.z = z;
                    if (Math.Abs(root.x - cell.x) > 1 || Math.Abs(root.z - cell.z) > 1)
                        yield return cell;
                }
            }
        }
    }
}