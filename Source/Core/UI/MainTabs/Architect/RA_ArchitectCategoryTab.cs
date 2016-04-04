using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_ArchitectCategoryTab : ArchitectCategoryTab
    {
        public RA_ArchitectCategoryTab(DesignationCategoryDef def) : base(def)
        {
            ReassembleDesignators();
        }

        public new void DesignationTabOnGUI()
        {
            if (DesignatorManager.SelectedDesignator != null)
            {
                DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, Screen.height - 35 - ((MainTabWindow_Architect)DefDatabase<MainTabDef>.GetNamed("Architect").Window).WinHeight - InfoRectHeight);
            }
            var startX = 210f;
            Gizmo selectedDesignator;
            GizmoGridDrawer.DrawGizmoGrid(def.resolvedDesignators.Cast<Gizmo>(), startX, out selectedDesignator);
            if (selectedDesignator == null && DesignatorManager.SelectedDesignator != null)
            {
                selectedDesignator = DesignatorManager.SelectedDesignator;
            }
            DoInfoBox(InfoRect, (Designator)selectedDesignator);
        }

        public new static Rect InfoRect => new Rect(0f, Screen.height - 35 - ((MainTabWindow_Architect)DefDatabase<MainTabDef>.GetNamed("Architect").Window).WinHeight - 230f, 200f, 230f);

        /// reassemble list of designators for current designator category.
        /// Replace vanilla Designator_Build objects with tweaked RA_Designator_Build ones
        public void ReassembleDesignators()
        {
            var buildableDefs = from buildableDef in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Union(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>())
                                where buildableDef.designationCategory == def.defName
                                select buildableDef;

            def.resolvedDesignators.RemoveAll(designator => designator.GetType() == typeof(Designator_Build));

            foreach (var buildableDef in buildableDefs)
            {
                def.resolvedDesignators.Add(new RA_Designator_Build(buildableDef));
            }
        }
    }
}