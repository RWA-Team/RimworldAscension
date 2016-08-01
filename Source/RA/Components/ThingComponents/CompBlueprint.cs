using RimWorld;
using Verse;

namespace RA
{
    public class CompBlueprint : CompUseEffect
    {
        public string ResearchName => (props as CompBlueprint_Properties).researchName;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            Find.ResearchManager.currentProj = ResearchProjectDef.Named(ResearchName);
            Find.ResearchManager.InstantFinish(Find.ResearchManager.currentProj, true);

            parent.Destroy();
        }
    }

    public class CompBlueprint_Properties : CompProperties_UseEffect
    {
        public string researchName = default(string);

        // Default requirement
        public CompBlueprint_Properties()
        {
            compClass = typeof(CompBlueprint);
        }
    }
}