using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class FloatMenuOption_Group : FloatMenuOption
    {
        public Designator_Build gizmo;
        public Texture2D backgroundTexture = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");
        public Color mouseOverColor = new Color(1f, 0.92f, 0.6f);

        public FloatMenuOption_Group(string label, Action action, Designator_Build gizmo,
            MenuOptionPriority priority = MenuOptionPriority.Medium, Action mouseoverGuiAction = null,
            Thing revalidateClickTarget = null)
            : base(label, action, priority, mouseoverGuiAction, revalidateClickTarget)
        {
            this.gizmo = gizmo;
        }

        public override bool DoGUI(Rect rect, bool colonistOrdering)
        {
            var result = gizmo.GizmoOnGUI(rect.position);
            if (result.State == GizmoState.Interacted)
                gizmo.ProcessInput(result.InteractEvent);
            
            return Widgets.ButtonInvisible(rect);
        }

        public new float RequiredHeight => UIUtil.GizmoSize;
    }
}