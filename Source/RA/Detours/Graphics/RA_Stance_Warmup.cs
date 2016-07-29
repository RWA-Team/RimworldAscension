using Verse;

namespace RA
{
    public class RA_Stance_Warmup : Stance_Warmup
    {
        public override void StanceDraw()
        {
            if (Find.Selector.IsSelected(stanceTracker.pawn))
            {
                var sway = 60; // sway -> degrees
                var range =
                    (stanceTracker.pawn.Position - (focusTarg.HasThing ? focusTarg.Thing.Position : focusTarg.Cell))
                        .LengthHorizontalSquared;

                RA_GenDraw.DrawShootingCone(stanceTracker.pawn, focusTarg, sway, range);

                GenDraw.DrawAimPie(stanceTracker.pawn, focusTarg, (int)(ticksLeft * pieSizeFactor), 0.2f);
            }
        }
    }
}
