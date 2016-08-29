using RimWorld;
using Verse;

namespace RA
{
    public class RA_SubEffecter_ProgressBar : SubEffecter_ProgressBar
    {
        public RA_SubEffecter_ProgressBar(SubEffecterDef def) : base(def)
        {
        }

        // make ProgressBar effecter deteriorate currently used tools
        public new void SubEffectTick(TargetInfo A, TargetInfo B)
        {
            if (mote == null)
            {
                mote = (MoteProgressBar)MoteMaker.MakeInteractionOverlay(def.moteDef, A, B);
                mote.exactScale.x = 0.68f;
                mote.exactScale.z = 0.12f;
            }
            
            // tool part injection
            Pawn pawn;
            if (B.HasThing && (pawn = B.Thing as Pawn) != null && pawn.Faction == Faction.OfPlayer)
            {
                CompTool toolComp;
                // pawn carries tool
                if ((toolComp = pawn.equipment.Primary?.TryGetComp<CompTool>()) != null)
                {
                    // tool is used for the corresponding job
                    if (toolComp.Allows("Mining") && MineUtility.MineableInCell(A.Cell) != null ||
                        toolComp.Allows("Woodchopping") && A.HasThing && A.Thing is Plant ||
                        toolComp.Allows("Construction") && A.HasThing && A.Thing is Building)
                    {
                        toolComp.ToolUseTick();
                    }
                }
            }
        }
    }
}