using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;


namespace RA
{
    public class ITab_TargetDummy_AllowedColonists : ITab
    {
        public Vector2 scrollPosition = Vector2.zero;

        public ITab_TargetDummy_AllowedColonists()
        {
            // same as info panel
            this.size = new Vector2(432f, 420f);
            this.labelKey = "Trainees";
        }

        public override bool IsVisible
        {
            get { return true; }
        }

        protected override void FillTab()
        {
            Building_Dummy dummy = this.SelThing as Building_Dummy;

            Text.Anchor = TextAnchor.MiddleCenter;

            float sideMargin = 25f;
            float pawnField_height = 30f;

            Rect labelRect = new Rect(0f, 0f, this.size.x - sideMargin, pawnField_height);
            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, "Pawns allowed to train here:");
            Text.Font = GameFont.Small;

            int pawnsCount = Find.ListerPawns.FreeColonistsSpawnedCount;
            int pawnsRowsCount = (pawnsCount % 2 == 1) ? (pawnsCount / 2 + 1) : (pawnsCount / 2);

            Rect tableRect = new Rect(0f, labelRect.height, this.size.x, this.size.y - labelRect.height - 3f);
            Rect scrollRect = new Rect(tableRect.x, tableRect.y, tableRect.width - sideMargin, pawnsRowsCount * pawnField_height - 3f);

            if (scrollRect.height > tableRect.height)
            {
                // add width of scroll bar
                tableRect.width -= 3f;
                Widgets.BeginScrollView(tableRect, ref scrollPosition, scrollRect);
            }

            int cellInd = 0;

            float currentX = scrollRect.x + sideMargin;
            float currentY = scrollRect.y;

            foreach (Pawn pawn in Find.ListerPawns.FreeColonistsSpawned)
            {
                bool allowedFlag = false;

                Rect currentPawn_rect = new Rect(currentX, currentY, (scrollRect.width - sideMargin * 2) / 2, pawnField_height);

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
