using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_Effecter : Effecter
    {
        public const int TicksPerOneDamage = GenDate.TicksPerHour;

        public RA_Effecter(EffecterDef def) : base(def)
        {
        }

        public new void Trigger(TargetInfo A, TargetInfo B)
        {
            base.Trigger(A, B);

            Pawn pawn;
            // colonist triggers effecter
            if (A.HasThing && (pawn = A.Thing as Pawn) != null && pawn.Faction == Faction.OfColony)
            {
                // has weapon with tags
                List<string> weaponTags;
                if (!(weaponTags = pawn.equipment?.Primary?.def.weaponTags).NullOrEmpty())
                {
                    // pawn carries tool
                    if (weaponTags.Exists(tag => tag.Contains("Tool")))
                    {
                        var tool = pawn.equipment.Primary;
                        // tool is used for the corresponding job
                        Thing mineable;
                        if ((mineable = MineUtility.MineableInCell(B.Cell)) != null && weaponTags.Exists(tag => tag.Contains("Mining")))
                        {
                            //tool.HitPoints -= tool.IsHashIntervalTick(TicksPerOneDamage)

                            /// tools should be made via comp for weapons
                            /// with worktype params, use params and so on
                        }
                    }

                }
            }
        }

    }
}
