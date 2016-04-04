using System;
using System.Collections.Generic;
using RA;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MD2
{
    public class JobDriver_Collect : JobDriver
    {
        public const TargetIndex CellInd = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(CellInd);

            yield return Toils_Reserve.Reserve(CellInd);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.ClosestTouch);
            //yield return Toils_General.Wait(500);
            //yield return Toils_MD2General.MakeAndSpawnThing(ThingDef.Named("MD2SandPile"), 100);
            //yield return Toils_MD2General.RemoveDesignationAtPosition(base.GetActor().jobs.curJob.GetTarget(CellInd).get_Cell(), DefDatabase<DesignationDef>.GetNamed("MD2CollectSand", true));
        }

        public Toil CollectResource(int ticksToCollect)
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
                Designation designation = Find.DesignationManager.DesignationAt(targetCell, DefDatabase<DesignationDef>.GetNamed("CollectClay"));
                if (designation != null)
                {
                    Find.DesignationManager.RemoveDesignation(designation);
                }

                // replace terrain
                {
                    if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("Mud"))
                        Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("Soil"));

                    if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("WaterShallow"))
                    {
                        Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("WaterDeep"));
                        foreach (var thing in Find.ThingGrid.ThingsAt(targetCell))
                            GenPlace.TryPlaceThing(thing, targetCell, ThingPlaceMode.Near);
                    }

                    if (Find.TerrainGrid.TerrainAt(targetCell) == TerrainDef.Named("Sand"))
                        Find.TerrainGrid.SetTerrain(targetCell, DefDatabase<TerrainDef>.GetNamed("Gravel"));
                }

                // produce resources
                {
                    Thing clayRed = ThingMaker.MakeThing(ThingDef.Named("ClumpClayRed"));
                    clayRed.stackCount = Rand.RangeInclusive(25, 75);
                    GenPlace.TryPlaceThing(clayRed, targetCell, ThingPlaceMode.Near);

                    if (Rand.Range(0, 1) > 0.5)
                    {
                        Thing clayWhite = ThingMaker.MakeThing(ThingDef.Named("ClumpClayWhite"));
                        clayWhite.stackCount = Rand.RangeInclusive(5, 10);
                        GenPlace.TryPlaceThing(clayWhite, targetCell, ThingPlaceMode.Near);
                    }
                }
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnCellMissingDesignation(CellInd, DefDatabase<DesignationDef>.GetNamed("CollectClay"));
            toil.WithEffect(() => EffecterDef.Named("CutStone"), CellInd);
            return toil;
        }
    }
}
