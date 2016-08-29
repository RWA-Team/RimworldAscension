using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_FertilizeLand : JobDriver
    {
        public const TargetIndex CellInd = TargetIndex.A;

        public int workTicks;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(CellInd);

            yield return Toils_Reserve.Reserve(CellInd);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.ClosestTouch);

            var targetCell = CurJob.targetA.Cell;
            pawn.jobs.curDriver.ticksLeftThisToil =
                Mathf.RoundToInt(targetCell.GetTerrain().GetStatValueAbstract(StatDefOf.WorkToMake) * pawn.GetStatValue(StatDefOf.PlantWorkSpeed));
            workTicks = pawn.jobs.curDriver.ticksLeftThisToil;

            yield return Fertilize();
        }

        public Toil Fertilize()
        {
            var targetCell = CurJob.targetA.Cell;

            var toil = new Toil();
            toil.tickAction = () =>
            {
                if (--pawn.jobs.curDriver.ticksLeftThisToil < 0)
                {
                    // replace terrain
                    if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("SoilCultivated"))
                        Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("SoilFertilized"));
                    if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("SoilRichCultivated"))
                        Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("SoilRichFertilized"));
                    if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("GravelCultivated"))
                        Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("GravelFertilized"));
                    if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("MarshyTerrainCultivated"))
                        Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("MarshyTerrainFertilized"));
                    if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("MossyTerrainCultivated"))
                        Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("MossyTerrainFertilized"));

                    ReadyForNextToil();
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOnCellMissingDesignation(CellInd, DefDatabase<DesignationDef>.GetNamed("Fertilize"));
            toil.WithEffect(() => EffecterDef.Named("CutStone"), CellInd);
            toil.PlaySustainerOrSound(() => DefDatabase<SoundDef>.GetNamedSilentFail("Recipe_Surgery"));
            toil.WithProgressBar(CellInd, () => 1f - (float)pawn.jobs.curDriver.ticksLeftThisToil / workTicks);
            return toil;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref workTicks, "workTicks");
        }
    }
}