using Verse;
using Verse.AI.Group;

namespace RA
{
    public class RA_Lord : Lord
    {
        public new void Notify_PawnTookDamage(Pawn victim, DamageInfo dinfo)
        {
            CheckTransitionOnSignal(new TriggerSignal
            {
                type = TriggerSignalType.PawnDamaged,
                pawn = victim,
                dinfo = dinfo
            });

            (CurLordToil as LordToil_DefendCaravan)?.Notify_PawnTookDamage(victim, dinfo);
        }

        public bool CheckTransitionOnSignal(TriggerSignal signal)
        {
            return ownedPawns.Count != 0 &&
                   Graph.transitions.Any(
                       transition => transition.sources.Contains(CurLordToil) && transition.CheckSignal(this, signal));
        }
    }
}
