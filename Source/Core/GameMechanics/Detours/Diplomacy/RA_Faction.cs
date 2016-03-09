using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Faction : Faction
    {
        public new void AffectGoodwillWith(Faction other, float goodwillChange)
        {
            if (goodwillChange > 0f && !def.appreciative)
            {
                return;
            }
            float value = GoodwillWith(other) + goodwillChange;
            FactionRelation factionRelation = RelationWith(other);
            factionRelation.goodwill = Mathf.Clamp(value, -100f, 100f);
            if (!this.HostileTo(other) && GoodwillWith(other) < -80f)
            {
                SetHostileTo(other, true);
                if (Game.Mode == GameMode.MapPlaying && Find.TickManager.TicksGame > 100 && other == OfColony)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeBad".Translate(), "RelationsBrokenDown".Translate(name), LetterType.BadNonUrgent);
                }
            }
            if (this.HostileTo(other) && GoodwillWith(other) > 0f)
            {
                SetHostileTo(other, false);
                if (Game.Mode == GameMode.MapPlaying && Find.TickManager.TicksGame > 100 && other == OfColony)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeGood".Translate(), "RelationsWarmed".Translate(name), LetterType.BadNonUrgent);
                }
            }
        }

        public new void Notify_MemberCaptured(Pawn member, Faction violator)
        {
            if (violator == this || this.HostileTo(violator))
            {
                return;
            }
            // TODO: set penalty maybe, not hostile?
            SetHostileTo(violator, true);
            Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeBad".Translate(), "RelationsBrokenCapture".Translate(member, name), LetterType.BadNonUrgent, member);
        }

        public new void SetHostileTo(Faction other, bool hostile)
        {
            FactionRelation factionRelation = RelationWith(other);
            if (hostile)
            {
                if (Game.Mode == GameMode.MapPlaying)
                {
                    foreach (Pawn current in Find.ListerPawns.PawnsInFaction(this).ToList())
                    {
                        if (current.HostFaction == other)
                        {
                            current.guest.SetGuestStatus(current.HostFaction, true);
                        }
                    }
                }
                if (!factionRelation.hostile)
                {
                    other.RelationWith(this).hostile = true;
                    factionRelation.hostile = true;
                    if (factionRelation.goodwill > -80f)
                    {
                        factionRelation.goodwill = -80f;
                    }
                }
            }
            else if (factionRelation.hostile)
            {
                other.RelationWith(this).hostile = false;
                factionRelation.hostile = false;
                if (factionRelation.goodwill < 0f)
                {
                    factionRelation.goodwill = 0f;
                }
            }
            if (Game.Mode == GameMode.MapPlaying)
            {
                List<Pawn> list = (from pa in Find.ListerPawns.AllPawns
                                   where pa.Faction == this || pa.Faction == other
                                   select pa).ToList();
                foreach (Pawn current2 in list)
                {
                    Find.ListerPawns.UpdateRegistryForPawn(current2);
                }
            }
        }
    }
}
