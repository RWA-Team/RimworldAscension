﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_DoBill : JobDriver
    {
        public float workLeft;
        public int billStartTick;

        public const TargetIndex BillGiverInd = TargetIndex.A;
        public const TargetIndex IngredientInd = TargetIndex.B;
        public const TargetIndex IngredientPlaceCellInd = TargetIndex.C;

        // same as vanilla
        public override string GetReport()
        {
            if (pawn.jobs.curJob.RecipeDef != null)
                return ReportStringProcessed(pawn.jobs.curJob.RecipeDef.jobString);
            return base.GetReport();
        }

        // same as vanilla
        public IBillGiver BillGiver
        {
            get
            {
                var giver = pawn.jobs.curJob.GetTarget(BillGiverInd).Thing as IBillGiver;

                if (giver == null)
                    throw new InvalidOperationException("DoBill on non-Billgiver.");

                return giver;
            }
        }

        // same as vanilla
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue(ref workLeft, "workLeft");
            Scribe_Values.LookValue(ref billStartTick, "billStartTick");
        }

        // duplicated to make changes
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Bill giver destroyed (only in bill using phase! Not in carry phase)
            AddEndCondition(() =>
            {
                var targ = GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (targ is Building && !targ.SpawnedInWorld)
                    return JobCondition.Incompletable;
                return JobCondition.Ongoing;
            });

            this.FailOnBurningImmobile(TargetIndex.A); //Bill giver, or product burning in carry phase

            this.FailOn(() =>
            {
                var billGiver = pawn.jobs.curJob.GetTarget(BillGiverInd).Thing as IBillGiver;

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
            var gotoBillGiver = Toils_Goto.GotoThing(BillGiverInd, PathEndMode.InteractionCell);

            //Reserve the bill giver and all the ingredients
            yield return Toils_Reserve.Reserve(BillGiverInd);
            yield return Toils_Reserve.ReserveQueue(IngredientInd);

            //Bind to bill if it should
            var bind = new Toil
            {
                initAction = () =>
                {
                    if (CurJob.targetQueueB != null && CurJob.targetQueueB.Count == 1)
                    {
                        var uft = CurJob.targetQueueB[0].Thing as UnfinishedThing;
                        if (uft != null)
                            uft.BoundBill = (Bill_ProductionWithUft) CurJob.bill;
                    }
                }
            };
            yield return bind;

            //Jump over ingredient gathering if there are no ingredients needed 
            yield return Toils_Jump.JumpIf(gotoBillGiver, () => CurJob.GetTargetQueue(IngredientInd).NullOrEmpty());

            //Gather ingredients
            {
                //Extract an ingredient into TargetB
                var extract = Toils_JobTransforms.ExtractNextTargetFromQueue(IngredientInd);
                yield return extract;

                //Get to ingredient and pick it up
                //Note that these fail cases must be on these toils, otherwise the recipe work fails if you stacked
                //   your targetB into another object on the bill giver square.
                var getToHaulTarget = Toils_Goto.GotoThing(IngredientInd, PathEndMode.ClosestTouch)
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

                var findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(BillGiverInd, IngredientInd,
                    IngredientPlaceCellInd);
                yield return findPlaceTarget;
                yield return Toils_Haul.PlaceHauledThingInCell(IngredientPlaceCellInd, findPlaceTarget, false);

                //Jump back if there is another ingredient needed
                //Can happen if you can't carry all the ingredients in one run
                yield return Toils_Jump.JumpIfHaveTargetInQueue(IngredientInd, extract);

            }

            //For it no ingredients needed, just go to the bill giver
            //This will do nothing if we took ingredients and are thus already at the bill giver
            yield return gotoBillGiver;

            //If the recipe calls for the use of an UnfinishedThing
            //Create that and convert our job to be a job about working on it

            // special case for burners
            if (CurJob.GetTarget(BillGiverInd).Thing is WorkTable_Fueled)
                yield return WaitUntilBurnerReady();

            // call duplicated to make changes
            yield return MakeUnfinishedThingIfNeeded();

            //Do the recipe
            //This puts the first product (if any) in targetC
            yield return DoRecipeWork().FailOnDespawnedOrForbiddenPlacedTargets();

            //Finish doing this recipe
            //Generate the products
            //Modify the job to store them

            // call duplicated to make changes
            yield return FinishRecipeAndStartStoringProduct();

            //If recipe has any products, store the first one
            if (!CurJob.RecipeDef.products.NullOrEmpty() || !CurJob.RecipeDef.specialProducts.NullOrEmpty())
            {
                //Reserve the storage cell
                yield return Toils_Reserve.Reserve(TargetIndex.B);

                var carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
                yield return carryToCell;

                yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);

                //Bit of a hack here
                //This makes the worker use a count including the one they just dropped
                //When determining whether to make the next item if the bill has "make until you have" marked.
                var recount = new Toil();
                recount.initAction = () =>
                {
                    var bill = recount.actor.jobs.curJob.bill as Bill_Production;
                    if (bill != null && bill.repeatMode == BillRepeatMode.TargetCount)
                        Find.ResourceCounter.UpdateResourceCounts();
                };
                yield return recount;
            }
        }

        // ignites burner to start consume fuel, if needed
        public Toil WaitUntilBurnerReady()
        {
            var burner = CurJob.GetTarget(TargetIndex.A).Thing as WorkTable_Fueled;

            var toil = new Toil
            {
                initAction = () =>
                {
                    if (burner == null)
                    {
                        return;
                    }
                    pawn.pather.StopDead();
                },
                tickAction = () =>
                {
                    if (burner.internalTemp > burner.compFueled.Properties.operatingTemp)
                        ReadyForNextToil();
                }
            };
            // fails if no more heat generation and temperature is no enough
            toil.FailOn(() => burner.currentFuelBurnDuration == 0 && !burner.UsableNow);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }

        // same as vanilla
        public static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                const float MaxDist = 8;
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var targetQueue = curJob.GetTargetQueue(ind);

                if (targetQueue.NullOrEmpty())
                    return;

                if (actor.carrier.CarriedThing == null)
                {
                    Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.");
                    return;
                }

                //Find an item in the queue matching what you're carrying
                for (var i = 0; i < targetQueue.Count; i++)
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
                    var numInHands = actor.carrier.CarriedThing?.stackCount;

                    //Determine num to take
                    var numToTake = curJob.numToBringList[i];
                    if (numToTake + numInHands > targetQueue[i].Thing.def.stackLimit)
                        numToTake = (int)(targetQueue[i].Thing.def.stackLimit - numInHands);

                    //Won't take any - skip
                    if (numToTake == 0)
                        continue;

                    //Remove the amount to take from the num to bring list
                    curJob.numToBringList[i] -= numToTake;

                    //Set me to go get it
                    curJob.maxNumToCarry = (int)(numInHands + numToTake);
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

        // duplicated to make changes
        public static Toil MakeUnfinishedThingIfNeeded()
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                if (!curJob.RecipeDef.UsesUnfinishedThing)
                {
                    return;
                }
                if (curJob.GetTarget(TargetIndex.B).Thing is UnfinishedThing)
                {
                    return;
                }
                Thing thing;
                var ingredients = ProcessedIngredients(curJob, out thing);
                // call duplicated to make changes
                var unfinishedThing = (UnfinishedThing)ThingMaker.MakeThing(curJob.RecipeDef.unfinishedThingDef, thing.def);
                unfinishedThing.Creator = actor;
                unfinishedThing.SetFactionDirect(actor.Faction);
                unfinishedThing.BoundBill = (Bill_ProductionWithUft)curJob.bill;
                unfinishedThing.ingredients = ingredients;
                var compColorable = unfinishedThing.TryGetComp<CompColorable>();
                if (compColorable != null)
                {
                    compColorable.Color = thing.DrawColor;
                }
                GenSpawn.Spawn(unfinishedThing, curJob.GetTarget(TargetIndex.A).Cell);
                curJob.SetTarget(TargetIndex.B, unfinishedThing);
                Find.Reservations.Reserve(actor, unfinishedThing);
            };
            return toil;
        }

        // duplicated to make changes
        public static Toil FinishRecipeAndStartStoringProduct()
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                Thing dominantIngredient;
                // call duplicated to make changes
                var ingredients = ProcessedIngredients(curJob, out dominantIngredient);
                var list = GenRecipe.MakeRecipeProducts(curJob.RecipeDef, actor, ingredients, dominantIngredient).ToList();
                curJob.bill.Notify_IterationCompleted(actor);
                if (list.Count == 0)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    return;
                }
                if (curJob.bill.GetStoreMode() == BillStoreMode.DropOnFloor)
                {
                    foreach (var thing in list)
                    {
                        if (!GenPlace.TryPlaceThing(thing, actor.Position, ThingPlaceMode.Near))
                        {
                            Log.Error(string.Concat(actor, " could not drop recipe product ", thing, " near ", actor.Position));
                        }
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    return;
                }
                if (list.Count > 1)
                {
                    for (var j = 1; j < list.Count; j++)
                    {
                        if (!GenPlace.TryPlaceThing(list[j], actor.Position, ThingPlaceMode.Near))
                        {
                            Log.Error(string.Concat(actor, " could not drop recipe product ", list[j], " near ", actor.Position));
                        }
                    }
                }
                list[0].SetPositionDirect(actor.Position);
                IntVec3 vec;
                if (WorkGiver_HaulGeneral.TryFindBestBetterStoreCellFor(list[0], actor, StoragePriority.Unstored, actor.Faction, out vec))
                {
                    actor.carrier.TryStartCarry(list[0]);
                    curJob.targetB = vec;
                    curJob.targetA = list[0];
                    curJob.maxNumToCarry = 99999;
                    return;
                }
                if (!GenPlace.TryPlaceThing(list[0], actor.Position, ThingPlaceMode.Near))
                {
                    Log.Error(string.Concat("Bill doer could not drop product ", list[0], " near ", actor.Position));
                }
                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
            };
            return toil;
        }

        // duplicated to make changes
        public static List<Thing> ProcessedIngredients(Job job, out Thing dominantIngredient)
        {
            // if use unfinished thing
            var uft = job.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
            if (uft != null)
            {
                dominantIngredient = uft.ingredients.First(ing => ing.def == uft.Stuff);
                var ingredients = uft.ingredients;
                uft.Destroy();
                job.placedTargets = null;
                return ingredients;
            }
            // if use placed ingridients
            var resources = new List<Thing>();
            if (job.placedTargets != null)
            {
                foreach (var target in job.placedTargets)
                {
                    var thing = target.Thing;
                    if (resources.Contains(thing))
                    {
                        Log.Error("Tried to add ingredient from job placed targets twice: " + thing);
                    }
                    else
                    {
                        resources.Add(thing);
                        if (thing.SpawnedInWorld)
                        {
                            var strippable = thing as IStrippable;
                            strippable?.Strip();
                        }
                        if (job.RecipeDef.UsesUnfinishedThing)
                        {
                            Find.DesignationManager.RemoveAllDesignationsOn(thing);
                            thing.DeSpawn();
                        }
                        else
                        {
                            thing.Destroy();
                        }
                    }
                }
            }
            job.placedTargets = null;
            dominantIngredient = resources.NullOrEmpty() ? null : GetDominantIngredient(resources, job.RecipeDef);
            return resources;
        }

        // duplicated to make changes
        public static Thing GetDominantIngredient(List<Thing> ingredients, RecipeDef recipe)
        {
            var disallowedFilter = recipe.defaultIngredientFilter;

            // checks if there are any stuff ingredients used which are not forbidden in defaultIngredientFilter
            foreach (var ingredient in ingredients)
            {
                if (ingredient.def.IsStuff)
                {
                    // check allowed stuff types
                    if (disallowedFilter != null)
                    {
                        if (!disallowedFilter.thingDefs.NullOrEmpty() && disallowedFilter.thingDefs.Contains(ingredient.def) || !disallowedFilter.categories.NullOrEmpty() && disallowedFilter.categories.Exists(cat => ingredient.def.thingCategories.Exists(cat2 => cat2.LabelCap == cat)))
                            continue;
                    }
                    return ingredient;
                }
            }
            // no suitable stuff ingridient found
            return ingredients.RandomElementByWeight(ing => ing.stackCount);
        }

        // duplicated to make changes
        public static Toil DoRecipeWork()
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                // call duplicated to make changes
                var jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
                var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                if (unfinishedThing != null && unfinishedThing.Initialized)
                {
                    jobDriver_DoBill.workLeft = unfinishedThing.workLeft;
                }
                else
                {
                    jobDriver_DoBill.workLeft = curJob.bill.recipe.WorkAmountTotal(unfinishedThing?.Stuff);
                    if (unfinishedThing != null)
                    {
                        unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
                    }
                }
                jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
                curJob.bill.Notify_DoBillStarted();
            };
            toil.tickAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                // call duplicated to make changes
                var jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
                var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                if (unfinishedThing != null && unfinishedThing.Destroyed)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }
                curJob.bill.Notify_PawnDidWork(actor);
                var billGiverWithTickAction = toil.actor.CurJob.GetTarget(TargetIndex.A).Thing as IBillGiverWithTickAction;
                billGiverWithTickAction?.BillTick();
                if (curJob.RecipeDef.workSkill != null)
                {
                    actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(0.11f * curJob.RecipeDef.workSkillLearnFactor);
                }
                var num = curJob.RecipeDef.workSpeedStat != null ? actor.GetStatValue(curJob.RecipeDef.workSpeedStat) : 1f;
                var building_WorkTable = jobDriver_DoBill.BillGiver as Building_WorkTable;
                if (building_WorkTable != null)
                {
                    num *= building_WorkTable.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor);
                }
                if (DebugSettings.fastCrafting)
                {
                    num *= 30f;
                }
                jobDriver_DoBill.workLeft -= num;
                if (unfinishedThing != null)
                {
                    unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
                }
                actor.GainComfortFromCellIfPossible();
                if (jobDriver_DoBill.workLeft <= 0f)
                {
                    jobDriver_DoBill.ReadyForNextToil();
                }
                if (curJob.bill.recipe.UsesUnfinishedThing)
                {
                    var num2 = Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick;
                    if (num2 >= 3000 && num2 % 1000 == 0)
                    {
                        actor.jobs.CheckForJobOverride();
                    }
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A);
            toil.WithSustainer(() => toil.actor.CurJob.bill.recipe.soundWorking);
            toil.FailOn(() => toil.actor.CurJob.bill.suspended);
            return toil;
        }
    }
}