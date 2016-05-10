using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ArchitectSense
{
    class FloatMenu_SubCategory : FloatMenu
    {
        private List<FloatMenuOption_SubCategory> _options;
        private Vector2 _optionSize;
        private bool _closeOnSelection;
        private const float _margin = 5f;

        // copypasta from base (privates, ugh)
        private Color baseColor;
        private int numColumns;
        
        /// <summary>
        /// Constructor for a floatmenu with configurable sizes, icons and background textures.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="title"></param>
        /// <param name="optionSize"></param>
        /// <param name="iconSize"></param>
        /// <param name="closeOnSelection"></param>
        public FloatMenu_SubCategory( List<FloatMenuOption_SubCategory> options,
                                     string title,
                                     Vector2 optionSize,
                                     bool closeOnSelection = false )
            : base( options.Select( opt => opt as FloatMenuOption ).ToList(), title )
        {
            _options          = options;
            _optionSize       = optionSize;
            _closeOnSelection = closeOnSelection;

            // need to redo column and size calculations because base isn't aware of our configurable sizes.
            // copypasta from base..ctor

            // set number of columns so the total height fits within game screen
            numColumns = 0;
            do
            {
                ++numColumns;
            }
            while( TotalHeight > Screen.height * .9 );
            currentWindowRect.size = InitialWindowSize;

            // first off, move float so it goes up from the mouse click instead of down
            currentWindowRect.y -= currentWindowRect.height;

            // tweak rect position to fit within window
            // note: we're assuming up, then right placement of buttons now. 
            if( currentWindowRect.xMax > (double)Screen.width )
                currentWindowRect.x = Screen.width - currentWindowRect.width;
            if ( currentWindowRect.yMin < 0f )
                currentWindowRect.y -= currentWindowRect.yMin;
            if( currentWindowRect.yMax > (double)Screen.height )
                currentWindowRect.y = Screen.height - currentWindowRect.height;
        }

        public override void DoWindowContents( Rect canvas )
        {
            // fall back on base implementation if options are not configurable options.
            if (_options == null)
                base.DoWindowContents( canvas );

            // define our own implementation, mostly copy-pasta with a few edits for option sizes
            // actual drawing is handled in OptionOnGUI.
            UpdateBaseColor();
            GUI.color = baseColor;
            Vector2 listRoot = ListRoot;
            Text.Font = GameFont.Small;
            int row = 0;
            int col = 0;

            if ( _options.NullOrEmpty() )
                return;

            Text.Font = GameFont.Tiny;
            foreach( FloatMenuOption_SubCategory option in _options.OrderByDescending( op => op.priority ) )
            {
                Rect optionRect = new Rect(listRoot.x + col * (_optionSize.x + _margin),
                                           listRoot.y + row * (_optionSize.y + _margin),
                                           _optionSize.x, _optionSize.y);

                // re-set transparent base color for each item.
                GUI.color = baseColor;
                if( option.OptionOnGUI( optionRect, baseColor ) )
                {
                    // click actions are handled in OptionOnGUI.
                    if( _closeOnSelection)
                        Find.WindowStack.TryRemove( this, true );
                    return;
                }
                row++;
                if( row >= ColumnMaxOptionCount )
                {
                    row = 0;
                    col++;
                }
            }
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }

        #region copypasta from Verse.FloatMenu
        private float TotalHeight
        {
            get
            {
                // the base constructor miscounts the number of options per column, overestimating it by one.
                return ColumnMaxOptionCount * (_optionSize.y + _margin);
            }
        }

        private float TotalWidth
        {
            get
            {
                return numColumns * (_optionSize.x + _margin);
            }
        }

        public override Vector2 InitialWindowSize
        {
            get
            {
                if( _options.NullOrEmpty() )
                    return new Vector2( 0.0f, 0.0f );
                return new Vector2( TotalWidth, TotalHeight );
            }
        }


        protected override WindowInitialPosition InitialPosition
        {
            get
            {
                return WindowInitialPosition.OnMouse;
            }
        }

        private float ColumnMaxOptionCount
        {
            get
            {
                if( options.Count % numColumns == 0 )
                    return options.Count / numColumns;
                return options.Count / numColumns + 1;
            }
        }

        private Vector2 ListRoot
        {
            get
            {
                return new Vector2( 4f, 0.0f );
            }
        }

        private Rect OverRect
        {
            get
            {
                return new Rect( ListRoot.x, ListRoot.y, TotalWidth, TotalHeight );
            }
        }

        private void UpdateBaseColor()
        {
            baseColor = Color.white;
            if( !vanishIfMouseDistant )
                return;
            Rect r = OverRect.ContractedBy( -12f);
            if( r.Contains( Event.current.mousePosition ) )
                return;
            float distanceFromRect = GenUI.DistFromRect(r, Event.current.mousePosition);
            baseColor = new Color( 1f, 1f, 1f, (float)( 1.0 - distanceFromRect / 200.0 ) );
            if( distanceFromRect <= 200.0 )
                return;
            Close( false );
            Cancel();
        }
        #endregion
    }
}
