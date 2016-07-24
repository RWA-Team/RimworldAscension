using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_ArchitectCategoryTab : ArchitectCategoryTab
    {
        public const float infoTabWidth = 200f;

        public RA_ArchitectCategoryTab(DesignationCategoryDef def) : base(def)
        {
            // TODO check if needed
            //ReassembleDesignators();
        }

        public new static Rect InfoRect => new Rect(0f, CurrentY, infoTabWidth, InfoRectHeight);

        public static float CurrentY
        {
            get
            {
                var architectWindow =
                    Find.WindowStack.Windows.FirstOrDefault(window => window is RA_MainTabWindow_Architect);
                var inspectWindow =
                    Find.WindowStack.Windows.FirstOrDefault(window => window is MainTabWindow_Inspect);

                return architectWindow?.windowRect.y - InfoRectHeight ?? inspectWindow.windowRect.y - (InfoRectHeight + UIUtil.ITabInvokeButtonHeight);
            }
        }

        // Replace vanilla Build designators with RA ones
        public void ReassembleDesignators()
        {
            def.ResolvedAllowedDesignators.ToList().RemoveAll(
                designator =>
                    designator.GetType() == typeof (Designator_Build) ||
                    designator.GetType() == typeof (RA_Designator_Build));

            var buildableDefs =
                from buildableDef in
                    DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>()
                        .Union(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>())
                where buildableDef.designationCategory == def.defName
                select buildableDef;
            foreach (var buildableDef in buildableDefs)
            {
                def.ResolvedAllowedDesignators.ToList().Add(new RA_Designator_Build(buildableDef));
            }
        }

        public new void DesignationTabOnGUI()
        {
            if (DesignatorManager.SelectedDesignator != null)
            {
                DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, CurrentY);
            }
            Gizmo selectedDesignator;
            GizmoGridDrawer.DrawGizmoGrid(def.ResolvedAllowedDesignators.ToList().Cast<Gizmo>(), infoTabWidth + 10f,
                out selectedDesignator);
            if (selectedDesignator == null && DesignatorManager.SelectedDesignator != null)
            {
                selectedDesignator = DesignatorManager.SelectedDesignator;
            }
            DoInfoBox(InfoRect, (Designator) selectedDesignator);
        }
    }
}