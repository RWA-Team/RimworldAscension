using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_DoBill_Research : WorkGiver_DoBill
    {
        // same as Research WorkGiver
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (Find.ResearchManager.currentProj == null)
                {
                    return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
                }
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }

        // same as Research WorkGiver
        public override bool ShouldSkip(Pawn pawn)
        {
            return Find.ResearchManager.currentProj == null || pawn.story.WorkTypeIsDisabled(WorkTypeDefOf.Research) || pawn.workSettings.GetPriority(WorkTypeDefOf.Research) == 0;
        }

        // same as Research WorkGiver
        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            return t.TryGetComp<CompResearcher>() != null && pawn.CanReserve(t);
        }

        public override Job JobOnThing(Pawn pawn, Thing researchBench)
        {
            var billGiver = researchBench as IBillGiver;
            Bill bill;

            // check if can generate bills
            if (billGiver == null)
            {
                return null;
            }
            // require no power or power available
            if (!billGiver.CurrentlyUsable())
            {
                return null;
            }

            if (!pawn.CanReserve(researchBench) ||
                !pawn.CanReach(researchBench.InteractionCell, PathEndMode.OnCell, Danger.Some) ||
                researchBench.IsBurning() || researchBench.IsForbidden(pawn))
            {
                return null;
            }

            // researchBench has added bills
            if (billGiver.BillStack.Count == 1)
            {
                // clear bill stack if research is finished or changed
                if (billGiver.BillStack[0].recipe.defName != Find.ResearchManager.currentProj.defName ||
                    Find.ResearchManager.IsFinished(ResearchProjectDef.Named(billGiver.BillStack[0].recipe.defName)))
                {
                    billGiver.BillStack.Clear();
                }
            }

            // Add research bill if it's not added already
            if (billGiver.BillStack.Count == 0)
            {
                bill = new Bill_Production(DefDatabase<RecipeDef>.GetNamed(Find.ResearchManager.currentProj.defName))
                {
                    suspended = true
                };
                // NOTE: why suspended???
                billGiver.BillStack.AddBill(bill);
            }
            else
            {
                bill = billGiver.BillStack[0];
            }

            if (!TryFindBestBillIngredients(bill, pawn, researchBench, chosenIngridiens))
            {
                if (FloatMenuMaker.making)
                {
                    JobFailReason.Is(MissingMaterialsTranslated);
                }
                return null;
            }
            bill.suspended = false;

            // Reserve research table
            if (pawn.CanReserveAndReach(researchBench, PathEndMode.OnCell, Danger.Some))
                pawn.Reserve(researchBench);

            // Reserve all rubbish on the table for the researcher
            {
                foreach (var cell in billGiver.IngredientStackCells)
                {
                    var thing = Find.ThingGrid.ThingAt(cell, ThingCategory.Item);
                    if (thing != null)
                    {
                        if (pawn.CanReserveAndReach(thing, PathEndMode.OnCell, Danger.Some))
                            pawn.Reserve(thing);
                    }
                }
            }

            // clear the work table
            var haulAside = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, billGiver, null);
            if (haulAside != null)
            {
                return haulAside;
            }
            
            // gather ingridients and do bill
            var doBill = new Job(JobDefOf.Research, researchBench)
            {
                targetQueueB = new List<TargetInfo>(chosenIngridiens.Count),
                numToBringList = new List<int>(chosenIngridiens.Count)
            };
            foreach (var ingridient in chosenIngridiens)
            {
                Log.Message(ingridient.thing + " " + ingridient.count);
                doBill.targetQueueB.Add(ingridient.thing);
                doBill.numToBringList.Add(ingridient.count);
            }
            doBill.haulMode = HaulMode.ToCellNonStorage;
            doBill.bill = bill;
            return doBill;
        }
    }
}
