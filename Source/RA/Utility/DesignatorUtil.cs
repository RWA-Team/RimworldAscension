using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public static class DesignatorUtil
    {
        public static Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);

        public static void CombineBuildDesignators()
        {
            foreach (var group in DefDatabase<DesignationGroupDef>.AllDefsListForReading)
            {
                var groupDesignators = new List<Designator_Build>();
                var categoryDef = DefDatabase<DesignationCategoryDef>.GetNamed(group.designationCategory);
                var firstFoundIndex = 0;

                // search for designators to group
                foreach (var defName in group.defNames)
                {
                    var buildableDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName) ??
                                       (BuildableDef) DefDatabase<TerrainDef>.GetNamed(defName);

                    var designatorToMove =
                        categoryDef.ResolvedDesignators()?
                            .FirstOrDefault(designator => (designator as Designator_Build)?.PlacingDef == buildableDef)
                            as Designator_Build;

                    if (firstFoundIndex == 0)
                        firstFoundIndex = categoryDef.ResolvedDesignators().IndexOf(designatorToMove);

                    if (designatorToMove != null)
                    {
                        groupDesignators.Add(designatorToMove);
                    }
                }

                // perform grouping if required
                if (groupDesignators.Count > 1)
                {
                    // remove grouped designators from original list
                    foreach (var designator in groupDesignators)
                    {
                        categoryDef.ResolvedDesignators().Remove(designator);
                    }

                    var firstDef = groupDesignators.FirstOrDefault().PlacingDef;

                    var desGroup = new Designator_Group
                    {
                        designators = groupDesignators,
                        defaultLabel = group.label,
                        defaultDesc = group.description,
                        iconProportions = new Vector2(1f, 1f),
                        iconDrawScale = 1f
                    };

                    if (group.iconPath != string.Empty)
                    {
                        desGroup.icon = ContentFinder<Texture2D>.Get(group.iconPath);
                    }
                    else
                    {
                        desGroup.icon = firstDef.uiIcon;

                        var thingDef = firstDef as ThingDef;
                        if (thingDef != null)
                        {
                            desGroup.iconProportions = thingDef.graphicData.drawSize;
                            desGroup.iconDrawScale = GenUI.IconDrawScale(thingDef);
                        }

                        if (firstDef is TerrainDef)
                            desGroup.iconTexCoords = new Rect(0.0f, 0.0f,
                                TerrainTextureCroppedSize.x/desGroup.icon.width,
                                TerrainTextureCroppedSize.y/desGroup.icon.height);
                    }

                    // insert group designator at location where first designator used to be.
                    categoryDef.ResolvedDesignators().Insert(firstFoundIndex, desGroup);
                }
            }
        }
    }
}