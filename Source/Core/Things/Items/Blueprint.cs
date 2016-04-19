using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class Blueprint : ThingWithComps, IUsable
    {
        public ResearchProjectDef researchDef;

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            researchDef = ResearchProjectDef.Named(this.TryGetComp<CompBlueprint>().Properties.researchName);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            if (Find.ResearchManager.IsFinished(researchDef))
            {
                yield return new FloatMenuOption("You already know everythig there", null);
            }
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                yield return new FloatMenuOption("Cannot use: incapable of reading", null);
            }
            else
            {
                Action action = () =>
                {
                    var job = new Job(DefDatabase<JobDef>.GetNamed("StudyBlueprint"), this);
                    pawn.drafter.TakeOrderedJob(job);
                };
                yield return new FloatMenuOption("Study " + Label, action);
            }
        }

        public void UsedBy(Pawn pawn)
        {
            Find.ResearchManager.currentProj = researchDef;
            Find.ResearchManager.InstantFinish(Find.ResearchManager.currentProj);

            Destroy();
        }
    }
}