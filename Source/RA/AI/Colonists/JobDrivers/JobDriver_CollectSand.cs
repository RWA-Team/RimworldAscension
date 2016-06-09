using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_CollectSand : JobDriver
    {
        public const float RareResourceSpawnChance = 0.5f;

        public const TargetIndex CellInd = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(CellInd);

            yield return Toils_Reserve.Reserve(CellInd);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.Touch);
            yield return CollectSand(GenDate.TicksPerHour*6);
        }

        public Toil CollectSand(int ticksToCollect)
        {
            var targetCell = CurJob.targetA.Cell;

            var toil = new Toil();
            toil.initAction = () =>
            {
                pawn.jobs.curDriver.ticksLeftThisToil =
                    Mathf.RoundToInt(ticksToCollect*pawn.GetStatValue(StatDef.Named("CollectingSpeed")));
            };
            toil.AddFinishAction(() =>
            {
                // remove designation
                var designation = Find.DesignationManager.DesignationAt(targetCell,
                    DefDatabase<DesignationDef>.GetNamed("CollectSand"));
                if (designation != null)
                {
                    Find.DesignationManager.RemoveDesignation(designation);
                }

                // replace terrain
                if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("Sand"))
                    Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("Gravel"));

                // spawn resources
                var SandYellow = ThingMaker.MakeThing(ThingDef.Named("PileSandYellow"));
                SandYellow.stackCount = Rand.RangeInclusive(20, 30);
                GenPlace.TryPlaceThing(SandYellow, targetCell, ThingPlaceMode.Near);

                // Rand.Value = Rand.Range(0, 1)
                if (Rand.Value < RareResourceSpawnChance)
                {
                    var SandWhite = ThingMaker.MakeThing(ThingDef.Named("PileSandWhite"));
                    SandWhite.stackCount = Rand.RangeInclusive(5, 10);
                    GenPlace.TryPlaceThing(SandWhite, targetCell, ThingPlaceMode.Near);
                }
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnCellMissingDesignation(CellInd, DefDatabase<DesignationDef>.GetNamed("CollectSand"));
            toil.WithEffect(() => EffecterDef.Named("CutStone"), CellInd);
            return toil;
        }
    }
}