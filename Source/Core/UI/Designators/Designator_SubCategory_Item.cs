using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArchitectSense
{
    public class Designator_SubCategory_Item : Designator_Build
    {
        // provide access to Designator_Build.entDef
        public static FieldInfo entDefFieldInfo = typeof (Designator_Build).GetField( "entDef",
                                                                              BindingFlags.NonPublic |
                                                                              BindingFlags.Instance );
        
        // default constructor from ThingDef, forwarded to base.
        public Designator_SubCategory_Item( ThingDef entDef ) : base( entDef ) {}

        // constructor from Designator_Build, links to constructor from ThingDef.
        public Designator_SubCategory_Item( Designator_Build designator ) : base( entDefFieldInfo.GetValue( designator ) as ThingDef ) {}

        public override GizmoResult GizmoOnGUI( Vector2 topLeft )
        {
            // start GUI.color is the transparency set by our floatmenu parent
            // store it, so we can apply it to all subsequent colours
            Color transparency = GUI.color;

            // below is 99% copypasta from Designator_Build, with minor naming changes and taking account of transparency.
            Rect buttonRect = new Rect(topLeft.x, topLeft.y, Width, 75f);
            bool mouseover = false;
            if( Mouse.IsOver( buttonRect ) )
            {
                mouseover = true;
                GUI.color = GenUI.MouseoverColor * transparency;
            }
            Texture2D tex = icon;
            if( tex == null )
                tex = BaseContent.BadTex;
            GUI.DrawTexture( buttonRect, BGTex );
            MouseoverSounds.DoRegion( buttonRect, SoundDefOf.MouseoverButtonCommand );
            GUI.color = IconDrawColor * transparency;
            Widgets.DrawTextureFitted( new Rect( buttonRect ), tex, iconDrawScale * 0.85f, iconProportions, iconTexCoords );
            GUI.color = Color.white * transparency;
            bool clicked = false;
            KeyCode keyCode = hotKey != null ? hotKey.MainKey : KeyCode.None;
            if( keyCode != KeyCode.None && !GizmoGridDrawer.drawnHotKeys.Contains( keyCode ) )
            {
                Widgets.Label( new Rect( buttonRect.x + 5f, buttonRect.y + 5f, 16f, 18f ), keyCode.ToString() );
                GizmoGridDrawer.drawnHotKeys.Add( keyCode );
                if( hotKey.KeyDownEvent )
                {
                    clicked = true;
                    Event.current.Use();
                }
            }
            if( Widgets.InvisibleButton( buttonRect ) )
                clicked = true;
            string labelCap = LabelCap;
            if( !labelCap.NullOrEmpty() )
            {
                float height = Text.CalcHeight(labelCap, buttonRect.width) - 2f;
                Rect rect2 = new Rect(buttonRect.x, (float) (buttonRect.yMax - (double) height + 12.0), buttonRect.width, height);
                GUI.DrawTexture( rect2, TexUI.GrayTextBG );
                GUI.color = Color.white * transparency;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label( rect2, labelCap );
                Text.Anchor = TextAnchor.UpperLeft;
            }
            GUI.color = Color.white;
            if( DoTooltip )
            {
                TipSignal tip = Desc;
                if( disabled && !disabledReason.NullOrEmpty() )
                {
                    TipSignal local = @tip;
                    local.text += "\n\nDISABLED: " + disabledReason;
                }
                TooltipHandler.TipRegion( buttonRect, tip );
            }
            if( !tutorHighlightTag.NullOrEmpty() )
                TutorUIHighlighter.HighlightOpportunity( tutorHighlightTag, buttonRect );


            if( clicked )
            {
                if( !disabled )
                    return new GizmoResult( GizmoState.Interacted, Event.current );
                if( !disabledReason.NullOrEmpty() )
                    Messages.Message( disabledReason, MessageSound.RejectInput );
                return new GizmoResult( GizmoState.Mouseover, null );
            }
            if( mouseover )
                return new GizmoResult( GizmoState.Mouseover, null );
            return new GizmoResult( GizmoState.Clear, null );
        }
    }
}
