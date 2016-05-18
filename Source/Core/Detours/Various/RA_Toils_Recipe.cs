using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public static class RA_Toils_Recipe
    {
        // added support for fuel burners
        public static Toil DoRecipeWork()
        {
            // research injection. initialized here to check if research project wan't change during work
            var startedResearch = Find.ResearchManager.currentProj;

            var toil = new Toil();
            toil.initAction = () =>
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var curDriver = actor.jobs.curDriver as JobDriver_DoBill;

                // burner support injection
                var burner = curJob.GetTarget(TargetIndex.A).Thing as WorkTableFueled;
                if (burner != null && burner.internalTemp > burner.compFueled.Properties.operatingTemp)
                {
                    curDriver.SetNextToil(RA_Toils.WaitUntilBurnerReady());
                    curDriver.ReadyForNextToil();
                }
                else
                {
                    var researchComp =
                        toil.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing.TryGetComp<CompResearcher>();
                    var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                    if (unfinishedThing != null && unfinishedThing.Initialized)
                    {
                        curDriver.workLeft = unfinishedThing.workLeft;
                    }
                    else
                    {
                        // research injection
                        if (researchComp != null)
                        {
                            (curJob.bill as Bill_Production).storeMode = BillStoreMode.DropOnFloor;
                            curDriver.workLeft = startedResearch.totalCost -
                                                 Find.ResearchManager.ProgressOf(startedResearch);
                        }
                        else
                        {
                            curDriver.workLeft = curJob.bill.recipe.WorkAmountTotal(unfinishedThing?.Stuff);
                        }

                        if (unfinishedThing != null)
                        {
                            unfinishedThing.workLeft = curDriver.workLeft;
                        }
                    }
                    curDriver.billStartTick = Find.TickManager.TicksGame;
                    curJob.bill.Notify_DoBillStarted();
                }
            };
            toil.tickAction = () =>
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;

                actor.GainComfortFromCellIfPossible();

                var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                if (unfinishedThing != null && unfinishedThing.Destroyed)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                curJob.bill.Notify_PawnDidWork(actor);

                var billGiverWithTickAction =
                    curJob.GetTarget(TargetIndex.A).Thing as IBillGiverWithTickAction;
                billGiverWithTickAction?.BillTick();

                if (curJob.RecipeDef.workSkill != null)
                {
                    actor.skills.GetSkill(curJob.RecipeDef.workSkill)
                        .Learn(LearnRates.XpPerTickRecipeBase*curJob.RecipeDef.workSkillLearnFactor);
                }

                var workProgress = (curJob.RecipeDef.workSpeedStat != null)
                    ? actor.GetStatValue(curJob.RecipeDef.workSpeedStat)
                    : 1f;

                var curDriver = actor.jobs.curDriver as JobDriver_DoBill;
                var building_WorkTable = curDriver.BillGiver as Building_WorkTable;
                if (building_WorkTable != null)
                {
                    workProgress *= building_WorkTable.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor);
                }

                if (DebugSettings.fastCrafting)
                {
                    workProgress *= 30f;
                }

                curDriver.workLeft -= workProgress;
                if (unfinishedThing != null)
                {
                    unfinishedThing.workLeft = curDriver.workLeft;
                }

                // research injection
                var researchComp = curJob.GetTarget(TargetIndex.A).Thing.TryGetComp<CompResearcher>();
                if (researchComp != null)
                {
                    if (Find.ResearchManager.currentProj != null && Find.ResearchManager.currentProj == startedResearch)
                    {
                        Find.ResearchManager.MakeProgress(workProgress, actor);
                    }
                    if (Find.ResearchManager.currentProj != startedResearch)
                    {
                        curDriver.ReadyForNextToil();
                    }
                }
                else if (curDriver.workLeft <= 0f)
                {
                    curDriver.ReadyForNextToil();
                }

                if (curJob.bill.recipe.UsesUnfinishedThing)
                {
                    var num2 = Find.TickManager.TicksGame - curDriver.billStartTick;
                    if (num2 >= 3000 && num2%1000 == 0)
                    {
                        actor.jobs.CheckForJobOverride();
                    }
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A);
            toil.WithSustainer(() => toil.actor.CurJob.bill.recipe.soundWorking);
            toil.WithProgressBar(TargetIndex.A, () =>
            {
                var actor = toil.actor;
                var curJob = actor.CurJob;
                var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                var researchComp = curJob.GetTarget(TargetIndex.A).Thing.TryGetComp<CompResearcher>();
                // research injection
                return researchComp == null
                    ? 1f - ((JobDriver_DoBill) actor.jobs.curDriver).workLeft/
                      curJob.bill.recipe.WorkAmountTotal(unfinishedThing?.Stuff)
                    : Find.ResearchManager.PercentComplete(startedResearch);
            });
            toil.FailOn(() => toil.actor.CurJob.bill.suspended);
            return toil;
        }

        // make recipe decide what result stuff to make based on defaultIngredientFilter as blocking Stuff types one
        public static Thing GetDominantIngredient(RecipeDef recipe, List<Thing> ingredients)
        {
            // checks if there are any stuff ingredients used which are not forbidden in defaultIngredientFilter
            // accepts any other stuff types
            var disallowedFilter = recipe.defaultIngredientFilter;
            if (disallowedFilter != null)
            {
                var dominantIngridient =
                    ingredients.Find(ingredient => ingredient.def.IsStuff && !disallowedFilter.Allows(ingredient.def));
                if (dominantIngridient != null)
                    return dominantIngridient;
            }
            // no suitable stuff ingridient found
            return ingredients.RandomElementByWeight(ing => ing.stackCount);
        }
    }
}