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

        // make effecter trigger deteriorate used tools
        public new void Trigger(TargetInfo A, TargetInfo B)
        {
            base.Trigger(A, B);

            Pawn pawn;
            // colonist triggers effecter
            if (A.HasThing && (pawn = A.Thing as Pawn) != null && pawn.Faction == Faction.OfColony)
            {
                CompTool toolComp;
                // pawn carries tool
                if ((toolComp = pawn.equipment.Primary.TryGetComp<CompTool>()) != null)
                {
                    // tool is used for the corresponding job
                    if (toolComp.Allows("Mining") && MineUtility.MineableInCell(B.Cell) != null ||
                        toolComp.Allows("PlantCutting") && B.HasThing && B.Thing is Plant ||
                        toolComp.Allows("Construction") && B.HasThing && B.Thing is Building)
                    {
                        toolComp.ToolUseTick();
                    }
                }
            }
        }
    }
}