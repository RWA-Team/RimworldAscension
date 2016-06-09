using RimWorld;
using Verse;

namespace RA
{
    public static class RA_PawnComponentsUtility
    {
        // assign Pawn_TraderTracker based on pawn caravan role, not mindState.wantsToTradeWithColony
        public static void AddAndRemoveDynamicComponents(Pawn pawn, bool actAsIfSpawned = false)
        {
            var flag = pawn.Faction != null && pawn.Faction.def == FactionDefOf.Colony;
            var flag2 = pawn.HostFaction != null && pawn.HostFaction.def == FactionDefOf.Colony;
            if (pawn.RaceProps.Humanlike && (!pawn.Dead || actAsIfSpawned))
            {
                if (pawn.GetCaravanRole() == TraderCaravanRole.Trader)
                {
                    if (pawn.trader == null)
                    {
                        pawn.trader = new Pawn_TraderTracker(pawn);
                    }
                }
                else
                {
                    pawn.trader = null;
                }
            }
            if (pawn.RaceProps.Humanlike)
            {
                if (flag)
                {
                    if (pawn.outfits == null)
                    {
                        pawn.outfits = new Pawn_OutfitTracker(pawn);
                    }
                    if (pawn.timetable == null)
                    {
                        pawn.timetable = new Pawn_TimetableTracker();
                    }
                    if ((pawn.Spawned || actAsIfSpawned) && pawn.drafter == null)
                    {
                        pawn.drafter = new Pawn_DraftController(pawn);
                    }
                }
                else
                {
                    pawn.drafter = null;
                }
            }
            if (flag || flag2)
            {
                if (pawn.playerSettings == null)
                {
                    pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                }
            }
            if (pawn.RaceProps.intelligence <= Intelligence.ToolUser && pawn.Faction != null && !pawn.RaceProps.IsMechanoid && pawn.training == null)
            {
                pawn.training = new Pawn_TrainingTracker(pawn);
            }
            pawn.needs?.AddOrRemoveNeedsAsAppropriate();
        }

    }
}
