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
                var groupDesignators = new List<Designator>();
                var categoryDef = DefDatabase<DesignationCategoryDef>.GetNamed(group.designationCategory);

                // search for designators to group
                foreach (var defName in group.defNames)
                {
                    var buildableDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName) ??
                                       (BuildableDef) DefDatabase<TerrainDef>.GetNamedSilentFail(defName);

                    Designator designatorToGroup = null;
                    if (buildableDef != null)
                    {
                        designatorToGroup =
                            categoryDef.ResolvedDesignators()?
                                .FirstOrDefault(designator
                                    => (designator as Designator_Build)?.PlacingDef == buildableDef);
                    }
                    else
                    {
                        designatorToGroup =
                            categoryDef.ResolvedDesignators()?
                                .FirstOrDefault(designator
                                    => GenTypes.GetTypeInAnyAssembly(defName) == designator.GetType());
                    }

                    if (designatorToGroup != null)
                    {
                        groupDesignators.Add(designatorToGroup);
                    }
                }

                // perform grouping if required
                if (groupDesignators.Any())
                {
                    // remove grouped designators from original list
                    foreach (var designator in groupDesignators)
                    {
                        categoryDef.ResolvedDesignators().Remove(designator);
                    }

                    //var firstDef = groupDesignators.FirstOrDefault().PlacingDef;

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
                    //else
                    //{
                    //    desGroup.icon = firstDef.uiIcon;

                    //    var thingDef = firstDef as ThingDef;
                    //    if (thingDef != null)
                    //    {
                    //        desGroup.iconProportions = thingDef.graphicData.drawSize;
                    //        desGroup.iconDrawScale = GenUI.IconDrawScale(thingDef);
                    //    }

                    //    if (firstDef is TerrainDef)
                    //        desGroup.iconTexCoords = new Rect(0.0f, 0.0f,
                    //            TerrainTextureCroppedSize.x/desGroup.icon.width,
                    //            TerrainTextureCroppedSize.y/desGroup.icon.height);
                    //}

                    categoryDef.ResolvedDesignators().Add(desGroup);
                }
            }
        }
    }
}