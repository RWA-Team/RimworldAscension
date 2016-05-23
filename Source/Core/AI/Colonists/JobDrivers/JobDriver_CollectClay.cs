using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_CollectClay : JobDriver
    {
        public const float RareResourceSpawnChance = 0.5f;
        public const TargetIndex CellInd = TargetIndex.A;

        public int collectDuration;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(CellInd);

            yield return Toils_Reserve.Reserve(CellInd);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.Touch);
            yield return CollectClay(GenDate.TicksPerHour*2);
        }

        public Toil CollectClay(int ticksToCollect)
        {
            var targetCell = CurJob.targetA.Cell;

            var toil = new Toil();
            toil.initAction = () =>
            {
                pawn.jobs.curDriver.ticksLeftThisToil =
                    Mathf.RoundToInt(ticksToCollect*pawn.GetStatValue(StatDef.Named("CollectingSpeed")));
                collectDuration = pawn.jobs.curDriver.ticksLeftThisToil;
            };
            toil.AddFinishAction(() =>
            {
                // remove designation
                var designation = Find.DesignationManager.DesignationAt(targetCell,
                    DefDatabase<DesignationDef>.GetNamed("CollectClay"));
                if (designation != null)
                {
                    Find.DesignationManager.RemoveDesignation(designation);
                }

                // replace terrain
                if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("Mud"))
                    Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("Soil"));
                if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("SoilRich"))
                    Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("Soil"));
                if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("WaterShallow"))
                {
                    Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("WaterDeep"));

                    var list = new List<Thing>(Find.ThingGrid.ThingsListAtFast(targetCell)
                        .Where(thing => thing.def.category == ThingCategory.Item ||
                                        thing.def.category == ThingCategory.Pawn));
                    foreach (var thing in list)
                    {
                        Thing dummy;
                        // despawn thing to spawn again with TryPlaceThing
                        thing.DeSpawn();
                        if (!GenPlace.TryPlaceThing(thing, thing.Position, ThingPlaceMode.Near, out dummy))
                        {
                            Log.Error("No free spot for " + thing);
                        }
                    }
                }

                // spawn resources
                var clayRed = ThingMaker.MakeThing(ThingDef.Named("ClumpClayGray"));
                clayRed.stackCount = Rand.RangeInclusive(25, 75);
                GenPlace.TryPlaceThing(clayRed, targetCell, ThingPlaceMode.Near);

                // Rand.Value = Rand.Range(0, 1)
                if (Rand.Value < RareResourceSpawnChance)
                {
                    var clayWhite = ThingMaker.MakeThing(ThingDef.Named("ClumpClayWhite"));
                    clayWhite.stackCount = Rand.RangeInclusive(5, 10);
                    GenPlace.TryPlaceThing(clayWhite, targetCell, ThingPlaceMode.Near);
                }
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnCellMissingDesignation(CellInd, DefDatabase<DesignationDef>.GetNamed("CollectClay"));
            toil.WithEffect(() => EffecterDef.Named("CutStone"), CellInd);
            toil.WithProgressBar(CellInd, () => 1f - (float) pawn.jobs.curDriver.ticksLeftThisToil/collectDuration);
            return toil;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref collectDuration, "collectDuration");
        }
    }
}