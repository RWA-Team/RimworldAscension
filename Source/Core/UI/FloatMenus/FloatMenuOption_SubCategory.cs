// ArchitectSense/FloatMenuOption_SubCategory.cs
// 
// Copyright Karel Kroeze, 2016.
// 
// Created 2016-02-15 23:59

using System;
using CommunityCoreLibrary;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArchitectSense
{
    internal class FloatMenuOption_SubCategory : FloatMenuOption
    {
        public Designator_Build gizmo;
        public Texture2D backgroundTexture = ContentFinder<Texture2D>.Get( "UI/Widgets/DesButBG" );
        public Color mouseOverColor = new Color( 1f, 0.92f, 0.6f );

        public FloatMenuOption_SubCategory( string label,
                                            Action action,
                                            Designator_Build gizmo,
                                            MenuOptionPriority priority = MenuOptionPriority.Medium,
                                            Action mouseoverGuiAction = null,
                                            Thing revalidateClickTarget = null )
            : base( label, action, priority, mouseoverGuiAction, revalidateClickTarget )
        {
            this.gizmo = gizmo;
        }

        public override bool OptionOnGUI( Rect rect, Color baseColor )
        {
            // considering we're trying to recreate the gizmo look and feel, we might as well steal the Gizmo.OnGUI
            GizmoResult x = gizmo.GizmoOnGUI( rect.position );
            if (x.State == GizmoState.Interacted )
                gizmo.ProcessInput( x.InteractEvent );
            
            // return clicks.
            return Widgets.InvisibleButton( rect );
        }
    }
}