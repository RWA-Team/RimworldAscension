using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class Alert_DoResearch : Alert_High
    {
        public override AlertReport Report
        {
            get
            {
                if (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("StartPack")) || Find.ListerBuildings.allBuildingsColonist.Any(building => building.TryGetComp<CompResearcher>() != null))
                {
                    return AlertReport.Inactive;
                }
                return AlertReport.Active;
            }
        }

        public Alert_DoResearch()
        {
            baseLabel = "Build research table";
            baseExplanation = "Having no other means to survive, you have to find a way to study your surroundings. Look for some clues to make a research table and examine things on it to learn something new.\n\nChecking ship wrecks might help you find something useful.";
        }
    }
}
