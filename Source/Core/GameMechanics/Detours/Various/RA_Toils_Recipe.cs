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
                var jobDriver_DoBill = (JobDriver_DoBill) actor.jobs.curDriver;

                // burner support injection
                var burner = curJob.GetTarget(TargetIndex.A).Thing as WorkTableFueled;
                if (burner != null && burner.internalTemp > burner.compFueled.Properties.operatingTemp)
                {
                    jobDriver_DoBill.SetNextToil(RA_Toils.WaitUntilBurnerReady());
                    jobDriver_DoBill.ReadyForNextToil();
                }
                else
                {
                    var researchComp =
                        toil.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing.TryGetComp<CompResearcher>();
                    var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                    if (unfinishedThing != null && unfinishedThing.Initialized)
                    {
                        // research injection
                        jobDriver_DoBill.workLeft = researchComp == null
                            ? unfinishedThing.workLeft
                            : startedResearch.totalCost -
                              Find.ResearchManager.ProgressOf(startedResearch);
                    }
                    else
                    {
                        // research injection
                        jobDriver_DoBill.workLeft = researchComp == null
                            ? curJob.bill.recipe.WorkAmountTotal(unfinishedThing?.Stuff)
                            : startedResearch.totalCost -
                              Find.ResearchManager.ProgressOf(startedResearch);
                        if (unfinishedThing != null)
                        {
                            unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
                        }
                    }
                    jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
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

                var jobDriver_DoBill = (JobDriver_DoBill) actor.jobs.curDriver;
                var building_WorkTable = jobDriver_DoBill.BillGiver as Building_WorkTable;
                if (building_WorkTable != null)
                {
                    workProgress *= building_WorkTable.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor);
                }

                if (DebugSettings.fastCrafting)
                {
                    workProgress *= 30f;
                }

                jobDriver_DoBill.workLeft -= workProgress;
                if (unfinishedThing != null)
                {
                    unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
                }

                //// research table support injection
                var researchComp = curJob.GetTarget(TargetIndex.A).Thing.TryGetComp<CompResearcher>();
                if (researchComp != null && Find.ResearchManager.currentProj != null &&
                    Find.ResearchManager.currentProj == startedResearch)
                {
                    Find.ResearchManager.MakeProgress(workProgress, actor);
                }

                if (jobDriver_DoBill.workLeft <= 0f)
                {
                    jobDriver_DoBill.ReadyForNextToil();
                }
                if (curJob.bill.recipe.UsesUnfinishedThing)
                {
                    var num2 = Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick;
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
                return 1f -
                       ((JobDriver_DoBill) actor.jobs.curDriver).workLeft/
                       curJob.bill.recipe.WorkAmountTotal(unfinishedThing?.Stuff);
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