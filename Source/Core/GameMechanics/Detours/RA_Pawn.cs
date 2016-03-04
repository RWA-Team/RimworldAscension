using RimWorld.SquadAI;
using Verse;

namespace RA
{
    class RA_Pawn : Pawn
    {
        public new void ExitMap()
        {
            Log.Message("EXIT!!!!!!!!");
            var squadBrain = this.GetSquadBrain();
            squadBrain?.Notify_PawnLost(this, PawnLostCondition.ExitedMap);
            carrier?.CarriedThing?.Destroy();

            if (HostFaction != null && guest != null && (guest.released || !IsPrisoner) && !Broken && health.hediffSet.BleedingRate < 0.001f && Faction.def.appreciative)
            {
                Messages.Message("MessagePawnExitMapRelationsGain".Translate(LabelBaseShort, Faction.name, 15f.ToString("F0")), MessageSound.Benefit);
                Faction.AffectGoodwillWith(HostFaction, 15f);
            }
            Destroy();
        }
    }
}