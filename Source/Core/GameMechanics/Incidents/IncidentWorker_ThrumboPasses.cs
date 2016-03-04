
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    // Copied from vanilla, bacause it uses IncidentMakerUtility class
    public class IncidentWorker_ThrumboPasses : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            IntVec3 intVec;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec))
            {
                return false;
            }
            var thrumbo = PawnKindDefOf.Thrumbo;
            var points = IncidentParmsUtility.GenerateThreatPointsParams().points;
            var num = GenMath.RoundRandom(points / thrumbo.combatPower);
            var max = Rand.RangeInclusive(2, 4);
            num = Mathf.Clamp(num, 1, max);
            var num2 = Rand.RangeInclusive(90000, 150000);
            IntVec3 invalid;
            if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(intVec, 10f, out invalid))
            {
                invalid = IntVec3.Invalid;
            }
            Pawn pawn = null;
            for (var i = 0; i < num; i++)
            {
                var loc = CellFinder.RandomClosewalkCellNear(intVec, 10);
                pawn = PawnGenerator.GeneratePawn(thrumbo, null);
                GenSpawn.Spawn(pawn, loc, Rot4.Random);
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num2;
                if (invalid.IsValid)
                {
                    pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(invalid, 10);
                }
            }
            Find.LetterStack.ReceiveLetter("LetterLabelThrumboPasses".Translate(thrumbo.label).CapitalizeFirst(), "LetterThrumboPasses".Translate(thrumbo.label), LetterType.Good, pawn);
            return true;
        }
    }
}
