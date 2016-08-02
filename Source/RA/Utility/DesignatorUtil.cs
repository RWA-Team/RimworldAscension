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

        public static void ModifyBuildDesignators()
        {
            foreach (var groupDef in DefDatabase<DesignationGroupDef>.AllDefsListForReading)
            {
                var groupDesignators = new List<Designator_Build>();
                var categoryDef = DefDatabase<DesignationCategoryDef>.GetNamed(groupDef.designationCategory);
                var firstFoundPosition = 2;

                // search for designators to group
                foreach (var defName in groupDef.defNames)
                {
                    var buildableDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName) ??
                                       (BuildableDef) DefDatabase<TerrainDef>.GetNamed(defName);

                    var designatorToMove =
                        categoryDef.ResolvedDesignators()?
                            .FirstOrDefault(designator => (designator as Designator_Build)?.PlacingDef == buildableDef)
                            as Designator_Build;

                    if (firstFoundPosition == 2)
                        firstFoundPosition = categoryDef.ResolvedDesignators().IndexOf(designatorToMove);

                    // if not null, add designator to the group, and remove from main category
                    if (designatorToMove != null)
                    {
                        groupDesignators.Add(new Designator_Build(buildableDef));
                        categoryDef.ResolvedDesignators().Remove(designatorToMove);
                    }
                }

                // check if any designators were added to the group
                if (!groupDesignators.NullOrEmpty())
                {
                    var desGroup = new Designator_Group
                    {
                        designators = groupDesignators,
                        defaultLabel = groupDef.label,
                        defaultDesc = groupDef.description
                    };

                    var firstDef = desGroup.designators.FirstOrDefault().PlacingDef;

                    desGroup.icon = firstDef.uiIcon;

                    var thingDef = firstDef as ThingDef;
                    if (thingDef != null)
                    {
                        desGroup.iconProportions = thingDef.graphicData.drawSize;
                        desGroup.iconDrawScale = GenUI.IconDrawScale(thingDef);
                    }
                    else
                    {
                        desGroup.iconProportions = new Vector2(1f, 1f);
                        desGroup.iconDrawScale = 1f;
                    }

                    if (firstDef is TerrainDef)
                        desGroup.iconTexCoords = new Rect(0.0f, 0.0f,
                            TerrainTextureCroppedSize.x/desGroup.icon.width,
                            TerrainTextureCroppedSize.y/desGroup.icon.height);

                    // insert group designator at location where first designator used to be.
                    categoryDef.ResolvedDesignators().Insert(firstFoundPosition, desGroup);
                }
            }
        }
    }
}