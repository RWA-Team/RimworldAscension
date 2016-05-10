using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace ArchitectSense
{
    internal class Designator_SubCategory : Designator
    {
        public static Vector2 SubCategoryIndicatorSize = new Vector2( 16f, 16f );
        public static Texture2D SubCategoryIndicatorTexture = ContentFinder<Texture2D>.Get( "UI/Icons/SubcategoryIndicator" );
        public List<Designator_SubCategory_Item> SubDesignators = new List<Designator_SubCategory_Item>();

        private PropertyInfo _iconDrawColorPropertyInfo = typeof (Designator_Build).GetProperty( "IconDrawColor",
                                                                                                 BindingFlags.NonPublic |
                                                                                                 BindingFlags.Instance );

        public List<Designator_SubCategory_Item> ValidSubDesignators
        {
            get
            {
                return SubDesignators.Where( designator => designator.Visible ).ToList();
            }
        }

        public override bool Visible => ValidSubDesignators.Count > 0;

        protected override Color IconDrawColor => (Color)_iconDrawColorPropertyInfo.GetValue( SubDesignators.First(), null );

        #region Methods

        public override AcceptanceReport CanDesignateCell( IntVec3 loc )
        {
            return false;
        }

        public override GizmoResult GizmoOnGUI( Vector2 topLeft )
        {
            GizmoResult val = base.GizmoOnGUI( topLeft );
            if ( ValidSubDesignators.Count == 1 )
                return val;
            Rect subCategoryIndicatorRect = new Rect( topLeft.x + this.Width - 20f, topLeft.y + 4f, SubCategoryIndicatorSize.x, SubCategoryIndicatorSize.y );
            GUI.DrawTexture( subCategoryIndicatorRect, SubCategoryIndicatorTexture );
            return val;
        }

        public override bool GroupsWith( Gizmo other )
        {
            return false;
        }

        public override void ProcessInput( Event ev )
        {
            // if only one option, immediately skip to that option's processinput
            if ( ValidSubDesignators.Count() == 1 )
            {
                ValidSubDesignators.First().ProcessInput( ev );
                return;
            }

            List<FloatMenuOption_SubCategory> options = new List<FloatMenuOption_SubCategory>();
            foreach ( Designator_Build designator in ValidSubDesignators )
            {
                options.Add( new FloatMenuOption_SubCategory( designator.LabelCap, delegate
                {
                    designator.ProcessInput( ev );
                }, designator ) );
            }
            Find.WindowStack.Add( new FloatMenu_SubCategory( options, null, new Vector2( 75, 75 ) ) );
        }

        #endregion Methods
    }
}