using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    [StaticConstructorOnStartup]
    public class Designator_Group : Designator
    {
        public static Texture2D GroupIndicatorTexture = ContentFinder<Texture2D>.Get("UI/Icons/GroupIndicator");
        public static Vector2 GroupIndicatorSize = new Vector2(16f, 16f);
        //public static PropertyInfo colorInfo = typeof(Designator_Build).GetProperty("IconDrawColor", BindingFlags.NonPublic | BindingFlags.Instance);

        public List<Designator> designators = new List<Designator>();

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) => false;

        // can't stack with other gizmoes
        public override bool GroupsWith(Gizmo other) => false;

        //protected override Color IconDrawColor => (Color) colorInfo.GetValue(designators.First(), null);

        public override bool Visible => designators.Any(designator => designator.Visible);

        // draws group indicator overlay
        public override GizmoResult GizmoOnGUI(Vector2 position)
        {
            var result = base.GizmoOnGUI(position);
            var indicatorRect = new Rect(position.x + Width - 20f, position.y + 4f, GroupIndicatorSize.x,
                GroupIndicatorSize.y);
            GUI.DrawTexture(indicatorRect, GroupIndicatorTexture);
            return result;
        }
        
        public override void ProcessInput(Event ev)
        {
            var options = designators.Where(designator => designator.Visible)
                    .Select(designator => new FloatMenuOption_Group(designator.LabelCap, () => designator.ProcessInput(ev), designator))
                    .ToList();

            Find.WindowStack.Add(new FloatMenu_Group(options));
        }
    }
}
