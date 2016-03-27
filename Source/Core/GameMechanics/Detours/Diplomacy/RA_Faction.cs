using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Faction : Faction
    {
        // allow AffectGoodwillWith with hidden factions; make factions hostile when relations are negative
        public new void AffectGoodwillWith(Faction other, float goodwillChange)
        {
            if (goodwillChange > 0f && !def.appreciative)
            {
                return;
            }
            var value = GoodwillWith(other) + goodwillChange;
            var factionRelation = RelationWith(other);
            factionRelation.goodwill = Mathf.Clamp(value, -100f, 100f);
            if (!this.HostileTo(other) && GoodwillWith(other) < 0f)
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

        public new void Notify_MemberTookDamage(Pawn member, DamageInfo dinfo)
        {
            if (dinfo.Instigator?.Faction == null || !dinfo.Def.externalViolence || this.HostileTo(dinfo.Instigator.Faction))
            {
                return;
            }
            if (member.Broken && member.BrokenStateDef.isAggro)
            {
                return;
            }
            var pawn = dinfo.Instigator as Pawn;
            if (pawn != null && pawn.Broken && pawn.BrokenStateDef == BrokenStateDefOf.Berserk)
            {
                return;
            }
            float num = Mathf.Min(100, dinfo.Amount);
            var goodwillChange = -1.3f * num;
            if (dinfo.Instigator.Faction == OfColony && this != OfColony)
            {
                AffectGoodwillWith(dinfo.Instigator.Faction, goodwillChange);
            }
        }

        public new void FactionTick()
        {
            if (Find.TickManager.TicksGame % 1001 == 0 && this != OfColony)
            {
                if (ColonyGoodwill < def.naturalColonyGoodwill.min)
                {
                    AffectGoodwillWith(OfColony, def.goodwillDailyGain * 0f);
                }
                else if (ColonyGoodwill > def.naturalColonyGoodwill.max)
                {
                    AffectGoodwillWith(OfColony, -def.goodwillDailyFall * 0f);
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
            var factionRelation = RelationWith(other);
            if (hostile)
            {
                if (Game.Mode == GameMode.MapPlaying)
                {
                    foreach (var current in Find.ListerPawns.PawnsInFaction(this).ToList())
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
                var list = (from pa in Find.ListerPawns.AllPawns
                                   where pa.Faction == this || pa.Faction == other
                                   select pa).ToList();
                foreach (var current2 in list)
                {
                    Find.ListerPawns.UpdateRegistryForPawn(current2);
                }
            }
        }
    }
}
