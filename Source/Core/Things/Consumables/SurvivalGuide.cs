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
            DefDatabase<ThingDef>.GetNamed("ResearchBench", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("Campfire", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("Door", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("Wall", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("TrapDeadfall", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("Grave", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("Stool", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("TableShort", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("TableLong", true).researchPrerequisite = null;
            DefDatabase<ThingDef>.GetNamed("PlantPot", true).researchPrerequisite = null;

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
