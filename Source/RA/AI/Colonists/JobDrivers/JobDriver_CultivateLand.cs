using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_CultivateLand : JobDriver
    {
        public const TargetIndex CellInd = TargetIndex.A;

        public int workTicks;

        public Dictionary<TerrainDef, TerrainDef> terrainReplacements = new Dictionary<TerrainDef, TerrainDef>
        {
            {TerrainDef.Named("Soil"), TerrainDef.Named("SoilCultivated")},
            {TerrainDef.Named("SoilRich"), TerrainDef.Named("SoilRichCultivated")},
            {TerrainDef.Named("Gravel"), TerrainDef.Named("GravelCultivated")},
            {TerrainDef.Named("MarshyTerrain"), TerrainDef.Named("MarshyTerrainCultivated")},
            {TerrainDef.Named("MossyTerrain"), TerrainDef.Named("MossyTerrainCultivated")}
        };

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(CellInd);

            yield return Toils_Reserve.Reserve(CellInd);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.ClosestTouch);
            yield return Cultivate();
        }

        public Toil Cultivate()
        {
            var targetCell = CurJob.targetA.Cell;

            var toil = new Toil();
            toil.initAction = () =>
            {
                workTicks = Mathf.RoundToInt(terrainReplacements[targetCell.GetTerrain()].GetStatValueAbstract(StatDefOf.WorkToMake) * pawn.GetStatValue(StatDefOf.PlantWorkSpeed));
                pawn.jobs.curDriver.ticksLeftThisToil = workTicks;
            };
            toil.tickAction = () =>
            {
                if (--pawn.jobs.curDriver.ticksLeftThisToil < 0)
                {
                    // remove designation
                    var designation = Find.DesignationManager.DesignationAt(targetCell,
                        DefDatabase<DesignationDef>.GetNamed("CultivateLand"));
                    if (designation != null)
                    {
                        Find.DesignationManager.RemoveDesignation(designation);
                    }

                    // replace terrain
                    Find.TerrainGrid.SetTerrain(targetCell, terrainReplacements[targetCell.GetTerrain()]);

                    ReadyForNextToil();
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOnCellMissingDesignation(CellInd, DefDatabase<DesignationDef>.GetNamed("CultivateLand"));
            toil.WithEffect(() => EffecterDef.Named("CutStone"), CellInd);
            toil.WithSustainer(() => DefDatabase<SoundDef>.GetNamedSilentFail("Recipe_Surgery"));
            toil.WithProgressBar(CellInd, () => 1f - (float) pawn.jobs.curDriver.ticksLeftThisToil/workTicks);
            return toil;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref workTicks, "workTicks");
        }
    }
}