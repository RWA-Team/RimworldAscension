
using UnityEngine;
using Verse;

namespace RA
{
    public class ITab_AllowedColonists : ITab
    {
        public Vector2 scrollPosition = Vector2.zero;

        public ITab_AllowedColonists()
        {
            // same as info panel
            size = new Vector2(432f, 420f);
            labelKey = "Trainees";
        }

        public override bool IsVisible => true;

        protected override void FillTab()
        {
            var dummy = SelThing as Dummy;

            Text.Anchor = TextAnchor.MiddleCenter;

            var sideMargin = 25f;
            var pawnField_height = 30f;

            var labelRect = new Rect(0f, 0f, size.x - sideMargin, pawnField_height);
            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, "Pawns allowed to train here:");
            Text.Font = GameFont.Small;

            var pawnsCount = Find.ListerPawns.FreeColonistsSpawnedCount;
            var pawnsRowsCount = pawnsCount % 2 == 1 ? pawnsCount / 2 + 1 : pawnsCount / 2;

            var tableRect = new Rect(0f, labelRect.height, size.x, size.y - labelRect.height - 3f);
            var scrollRect = new Rect(tableRect.x, tableRect.y, tableRect.width - sideMargin, pawnsRowsCount * pawnField_height - 3f);

            if (scrollRect.height > tableRect.height)
            {
                // add width of scroll bar
                tableRect.width -= 3f;
                Widgets.BeginScrollView(tableRect, ref scrollPosition, scrollRect);
            }

            var cellInd = 0;

            var currentX = scrollRect.x + sideMargin;
            var currentY = scrollRect.y;

            foreach (var pawn in Find.ListerPawns.FreeColonistsSpawned)
            {
                var allowedFlag = false;

                var currentPawn_rect = new Rect(currentX, currentY, (scrollRect.width - sideMargin * 2) / 2, pawnField_height);

                // check if flag should be set already
                if (dummy.allowedPawns.Contains(pawn))
                    allowedFlag = true;

                Widgets.LabelCheckbox(currentPawn_rect, pawn.LabelCap, ref allowedFlag);

                // check if flag set to "true"
                if (allowedFlag && !dummy.allowedPawns.Contains(pawn))
                    dummy.allowedPawns.Add(pawn);

                // check if flag set to "false"
                if (!allowedFlag && dummy.allowedPawns.Contains(pawn))
                    dummy.allowedPawns.Remove(pawn);

                if (cellInd++ % 2 == 1)
                {
                    currentY += pawnField_height;
                    currentX = scrollRect.x + sideMargin;
                }
                else
                    currentX += currentPawn_rect.width + sideMargin;
            }
            if (scrollRect.height > tableRect.height)
                Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
