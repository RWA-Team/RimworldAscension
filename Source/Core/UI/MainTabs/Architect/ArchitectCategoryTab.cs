using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class ArchitectCategoryTab
    {
        public const float InfoRectHeight = 230f;

        public DesignationCategoryDef def;

        public static Rect InfoRect => new Rect(0f, Screen.height - 35 - ((MainTabWindow_Architect)MainTabDefOf.Architect.Window).WinHeight - 230f, 200f, 230f);

        public ArchitectCategoryTab(DesignationCategoryDef def)
        {
            this.def = def;
            ModifyDesignators();
        }

        // used to modify Designator_Build
        public void ModifyDesignators()
        {
            var enumerable = from tDef in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>())
                                                   where tDef.designationCategory == def.defName
                                                   select tDef;
            def.resolvedDesignators.RemoveAll(designator => designator.GetType() == typeof(RimWorld.Designator_Build));
            foreach (var current in enumerable)
            {
                def.resolvedDesignators.Add(new Designator_Build(current));
            }
        }

        public void PanelClosing()
        {
            DesignatorManager.Deselect();
        }

        public void DesignationTabOnGUI()
        {
            if (DesignatorManager.SelectedDesignator != null)
            {
                DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, Screen.height - 35 - ((MainTabWindow_Architect)MainTabDefOf.Architect.Window).WinHeight - 230f);
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

        public void DoInfoBox(Rect infoRect, Designator designator)
        {
            Find.WindowStack.ImmediateWindow(32520, infoRect, WindowLayer.GameUI, delegate
            {
                if (designator != null)
                {
                    var position = infoRect.AtZero().ContractedBy(7f);
                    GUI.BeginGroup(position);
                    var rect = new Rect(0f, 0f, position.width, 999f);
                    Text.Font = GameFont.Small;
                    Widgets.Label(rect, designator.LabelCap);
                    var num = 24f;
                    designator.DrawPanelReadout(ref num, position.width);
                    var rect2 = new Rect(0f, num, position.width, position.height - num);
                    var desc = designator.Desc;
                    GenText.SetTextSizeToFit(desc, rect2);
                    Widgets.Label(rect2, desc);
                    GUI.EndGroup();
                }
            });
        }
    }
}