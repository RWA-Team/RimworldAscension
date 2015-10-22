using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace RA.JobDrivers
{
    public class JobDriver_DoBill_Research : JobDriver
    {
        public const TargetIndex BillGiverInd = TargetIndex.A;
        public const TargetIndex IngredientInd = TargetIndex.B;
        public const TargetIndex IngredientPlaceCellInd = TargetIndex.C;

        public override string GetReport()
        {
            if (pawn.jobs.curJob.RecipeDef != null)
                return ReportStringProcessed(pawn.jobs.curJob.RecipeDef.jobString);
            else
                return base.GetReport();
        }

        public IBillGiver BillGiver
        {
            get
            {
                IBillGiver giver = pawn.jobs.curJob.GetTarget(BillGiverInd).Thing as IBillGiver;

                if (giver == null)
                    throw new InvalidOperationException("DoBill on non-Billgiver.");

                return giver;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Bill giver destroyed (only in bill using phase! Not in carry phase)
            AddEndCondition(() =>
                {
                    Building_WorkTable researchBench = CurJob.GetTarget(TargetIndex.A).Thing as Building_WorkTable;
                    if (!researchBench.SpawnedInWorld)
                        return JobCondition.Incompletable;
                    return JobCondition.Ongoing;
                });

            this.FailOnBurningImmobile(TargetIndex.A);	//Bill giver, or product burning in carry phase

            this.FailOn(() =>
            {
                IBillGiver billGiver = CurJob.GetTarget(BillGiverInd).Thing as IBillGiver;

                //conditions only apply during the billgiver-use phase
                if (billGiver != null)
                {
                    if (pawn.jobs.curJob.bill.DeletedOrDereferenced)
                        return true;

                    if (!billGiver.CurrentlyUsable())
                        return true;
                }
                return false;
            });

            //This toil is yielded later
            Toil gotoBillGiver = Toils_Goto.GotoThing(BillGiverInd, PathEndMode.InteractionCell);

            //Reserve the bill giver and all the ingredients
            yield return Toils_Reserve.Reserve(BillGiverInd);
            yield return Toils_Reserve.ReserveQueue(IngredientInd);

            //Jump over ingredient gathering if there are no ingredients needed 
            yield return Toils_Jump.JumpIf(gotoBillGiver, () => CurJob.GetTargetQueue(IngredientInd).NullOrEmpty());

            //Gather ingredients
            {
                //Extract an ingredient into TargetB
                Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(IngredientInd);
                yield return extract;

                //Get to ingredient and pick it up
                //Note that these fail cases must be on these toils, otherwise the recipe work fails if you stacked
                //   your targetB into another object on the bill giver square.
                Toil getToHaulTarget = Toils_Goto.GotoThing(IngredientInd, PathEndMode.ClosestTouch)
                                        .FailOnDespawned(IngredientInd)
                                        .FailOnForbidden(IngredientInd);
                yield return getToHaulTarget;

                yield return Toils_Haul.StartCarryThing(IngredientInd);

                //Jump to pick up more in this run if we're collecting from multiple stacks at once
                //Todo bring this back
                yield return JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);

                //Carry ingredient to the bill giver and put it on the square
                yield return Toils_Goto.GotoThing(BillGiverInd, PathEndMode.InteractionCell)
                                        .FailOnDestroyed(IngredientInd);

                Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(BillGiverInd, IngredientInd, IngredientPlaceCellInd);
                yield return findPlaceTarget;
                yield return Toils_Haul.PlaceHauledThingInCell(IngredientPlaceCellInd,
                                                                nextToilOnPlaceFailOrIncomplete: findPlaceTarget,
                                                                storageMode: false);
                //Jump back if there is another ingredient needed
                //Can happen if you can't carry all the ingredients in one run
                yield return Toils_Jump.JumpIfHaveTargetInQueue(IngredientInd, extract);
            }

            //For it no ingredients needed, just go to the bill giver
            //This will do nothing if we took ingredients and are thus already at the bill giver
            yield return gotoBillGiver;

            //Do the recipe
            //This puts the first product (if any) in targetC
            yield return DoResearch().FailOnDespawnedOrForbiddenPlacedTargets();

            //Reserve the storage cell
            yield return Toils_Reserve.Reserve(TargetIndex.B);

            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return carryToCell;

            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
        }

        public static Toil DoResearch()
        {
            Toil toil = new Toil();
            ResearchProjectDef startedResearch = Find.ResearchManager.currentProj;

            toil.tickAction = () =>
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver_DoBill_Research driver = ((JobDriver_DoBill_Research)actor.jobs.curDriver);

                PawnUtility.GainComfortFromCellIfPossible(actor);

                //Learn
                actor.skills.GetSkill(SkillDefOf.Research).Learn(LearnRates.XpPerTickRecipeBase * curJob.RecipeDef.workSkillLearnFactor);

                // Make progress
                float progressValue = actor.GetStatValue(StatDefOf.ResearchSpeed);

                Building_WorkTable researchTable = driver.BillGiver as Building_WorkTable;
                progressValue *= researchTable.GetStatValue(StatDefOf.ResearchSpeedFactor);

                if (DebugSettings.fastResearch)
                    progressValue *= 30;

                if (Find.ResearchManager.currentProj == null || Find.ResearchManager.currentProj != startedResearch)
                {
                    Log.Message("End or change");
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
                else
                {
                    Find.ResearchManager.MakeProgress(progressValue);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A);
            toil.WithSustainer(() => toil.actor.CurJob.bill.recipe.soundWorking);
            toil.FailOn(() => toil.actor.CurJob.bill.suspended);

            return toil;
        }

        public static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
            {
                const float MaxDist = 8;
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                List<TargetInfo> targetQueue = curJob.GetTargetQueue(ind);

                if (targetQueue.NullOrEmpty())
                    return;

                if (actor.carrier.CarriedThing == null)
                {
                    Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.");
                    return;
                }

                //Find an item in the queue matching what you're carrying
                for (int i = 0; i < targetQueue.Count; i++)
                {
                    //Can't use item - skip
                    if (!GenAI.CanUseItemForWork(actor, targetQueue[i].Thing))
                        continue;

                    //Cannot stack with thing in hands - skip
                    if (!targetQueue[i].Thing.CanStackWith(actor.carrier.CarriedThing))
                        continue;

                    //Too far away - skip
                    if ((actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared > MaxDist * MaxDist)
                        continue;

                    //Determine num in hands
                    int numInHands = (actor.carrier.CarriedThing == null) ? 0 : actor.carrier.CarriedThing.stackCount;

                    //Determine num to take
                    int numToTake = curJob.numToBringList[i];
                    if (numToTake + numInHands > targetQueue[i].Thing.def.stackLimit)
                        numToTake = targetQueue[i].Thing.def.stackLimit - numInHands;

                    //Won't take any - skip
                    if (numToTake == 0)
                        continue;

                    //Remove the amount to take from the num to bring list
                    curJob.numToBringList[i] -= numToTake;

                    //Set me to go get it
                    curJob.maxNumToCarry = numInHands + numToTake;
                    curJob.SetTarget(ind, targetQueue[i].Thing);

                    //Remove from queue if I'm going to take all
                    if (curJob.numToBringList[i] == 0)
                    {
                        curJob.numToBringList.RemoveAt(i);
                        targetQueue.RemoveAt(i);
                    }

                    actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
                    return;
                }

            };

            return toil;
        }
    }
}