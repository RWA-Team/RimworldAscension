using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RA
{
    public class CompTool : ThingComp
    {
        public int wearTicks;

        public CompTool_Properties Props => (CompTool_Properties)props;
        
        public bool Allows(string workType)
        {
            return Props.workTypes.Contains(workType);
        }

        public void ToolUseTick()
        {
            //Log.Message("ticks " + wearTicks);
            if (wearTicks-- <= 0) parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1));
        }

        public override void PostSpawnSetup()
        {
            wearTicks = Props.workTicksPerHealthPercent;
            //Log.Message("initial ticks " + wearTicks);
        }

        // adds work designators to pawns of tool equipped
        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            foreach (var worktype in Props.workTypes)
            {
                switch (worktype)
                {
                    case "Mining":
                        yield return new Designator_Mine();
                        break;
                    case "PlantCutting":
                        yield return new Designator_PlantsHarvestWood();
                        break;
                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.LookValue(ref wearTicks, "wearTicks");
            //Log.Message("restored ticks " + wearTicks);
        }
    }

    public class CompTool_Properties : CompProperties
    {
        public List<string> workTypes = new List<string>();
        public int workTicksPerHealthPercent;

        public CompTool_Properties()
        {
            compClass = typeof(CompTool);
        }
    }
}