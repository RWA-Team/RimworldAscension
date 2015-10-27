using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class SurvivalGuide : ThingWithComps, IUsable
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            if (!pawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("Cannot use: no path", null);
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                yield return new FloatMenuOption("Cannot use: incapable of reading", null);
            }
            else if (!pawn.CanReserve(this, 1))
            {
                yield return new FloatMenuOption("Cannot use: reserved", null);
            }
            else
            {
                Action action = () =>
                {
                    Job job = new Job(DefDatabase<JobDef>.GetNamed("UseSurvivalGuide"), this);
                    pawn.drafter.TakeOrderedJob(job);
                };
                yield return new FloatMenuOption("Examine " + this.LabelCap, action);
            }
        }

        public void UsedBy(Pawn pawn)
        {
            ThingDef.Named("ResearchBench").researchPrerequisite = null;
            ThingDef.Named("Campfire").researchPrerequisite = null;
            ThingDef.Named("Door").researchPrerequisite = null;
            ThingDef.Named("Wall").researchPrerequisite = null;
            ThingDef.Named("TrapDeadfall").researchPrerequisite = null;
            ThingDef.Named("Grave").researchPrerequisite = null;
            ThingDef.Named("Stool").researchPrerequisite = null;
            ThingDef.Named("TableShort").researchPrerequisite = null;
            ThingDef.Named("TableLong").researchPrerequisite = null;
            ThingDef.Named("PlantPot").researchPrerequisite = null;

            DefDatabase<TerrainDef>.GetNamed("TileSandstone", true).researchPrerequisite = null;
            DefDatabase<TerrainDef>.GetNamed("TileGranite", true).researchPrerequisite = null;
            DefDatabase<TerrainDef>.GetNamed("TileLimestone", true).researchPrerequisite = null;
            DefDatabase<TerrainDef>.GetNamed("TileSlate", true).researchPrerequisite = null;
            DefDatabase<TerrainDef>.GetNamed("TileMarble", true).researchPrerequisite = null;
            DefDatabase<TerrainDef>.GetNamed("WoodPlankFloor", true).researchPrerequisite = null;

            Messages.Message("You learned the basics", MessageSound.Benefit);

            this.Destroy(DestroyMode.Vanish);
        }
    }
}
