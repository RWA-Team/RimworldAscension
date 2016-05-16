using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RA
{
    public class RA_IncidentWorker_VisitorGroup : IncidentWorker_VisitorGroup
    {
        public override bool TryExecute(IncidentParms parms)
        {
            if (!TryResolveParms(parms))
            {
                return false;
            }
            var list = SpawnPawns(parms);
            if (list.Count == 0)
            {
                return false;
            }
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out chillSpot);
            var lordJob = new LordJob_VisitColony(parms.faction, chillSpot);
            LordMaker.MakeNewLord(parms.faction, lordJob, list);
            string label;
            string text2;
            if (list.Count == 1)
            {
                label = "LetterLabelSingleVisitorArrives".Translate();
                text2 = "SingleVisitorArrives".Translate(list[0].story.adulthood.title.ToLower(), parms.faction.name,
                    list[0].Name);
                text2 = text2.AdjustedFor(list[0]);
            }
            else
            {
                var text3 = string.Empty;
                label = "LetterLabelGroupVisitorsArrive".Translate();
                text2 = "GroupVisitorsArrive".Translate(parms.faction.name, text3);
            }
            Find.LetterStack.ReceiveLetter(label, text2, LetterType.Good, list[0]);
            return true;
        }
    }
}
