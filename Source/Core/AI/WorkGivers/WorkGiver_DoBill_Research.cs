using System.Collections.Generic;
using System.Linq;

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
            IBillGiver billGiver = researchBench as IBillGiver;
            Bill bill = null;

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

            if (!pawn.CanReserve(researchBench, 1) || !pawn.CanReach(researchBench.InteractionCell, PathEndMode.OnCell, Danger.Some, false) || researchBench.IsBurning() || researchBench.IsForbidden(pawn))
            {
                return null;
            }

            // researchBench has added bills
            if (billGiver.BillStack.Count == 1)
            {
                // clear bill stack if research is finished or changed
                if (billGiver.BillStack[0].recipe.defName != Find.ResearchManager.currentProj.defName || Find.ResearchManager.IsFinished(ResearchProjectDef.Named(billGiver.BillStack[0].recipe.defName)))
                {
                    billGiver.BillStack.Clear();
                }
            }

            // Add research bill if it's not added already
            if (billGiver.BillStack.Count == 0)
            {
                bill = new Bill_Production(DefDatabase<RecipeDef>.GetNamed(Find.ResearchManager.currentProj.defName));
                // NOTE: why suspended???
                bill.suspended = true;
                billGiver.BillStack.AddBill(bill);
            }
            else
            {
                bill = billGiver.BillStack[0];
            }

            if (!TryFindBestBillIngredients(bill, pawn, researchBench, this.chosenIngThings))
            {
                if (FloatMenuMaker.making)
                {
                    JobFailReason.Is(MissingMaterialsTranslated);
                }
                return null;
            }
            else
                bill.suspended = false;
            
            // Reserve research table
            if (pawn.CanReserveAndReach(researchBench, PathEndMode.OnCell, Danger.Some, 1))
                ReservationUtility.Reserve(pawn, researchBench, 1);

            // Reserve all rubbish on the table for the researcher
            {
                foreach (IntVec3 cell in billGiver.IngredientStackCells)
                {
                    Thing thing = Find.ThingGrid.ThingAt(cell, ThingCategory.Item);
                    if (thing != null)
                    {
                        if (pawn.CanReserveAndReach(thing, PathEndMode.OnCell, Danger.Some, 1))
                            ReservationUtility.Reserve(pawn, thing, 1);
                    }
                }
            }

            // clear the work table
            Job haulAside = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, billGiver, null);
            if (haulAside != null)
            {
                return haulAside;
            }

            // gather ingridients and do bill
            Job doBill = new Job(JobDefOf.Research, researchBench);
            doBill.targetQueueB = new List<TargetInfo>(this.chosenIngThings.Count);
            doBill.numToBringList = new List<int>(this.chosenIngThings.Count);
            for (int k = 0; k < this.chosenIngThings.Count; k++)
            {
                // pre reservation is made to assure that current researcher is cleaning the bench, not some other pawn with research skill
                ReservationUtility.Reserve(pawn, this.chosenIngThings[k].thing, 1);
                doBill.targetQueueB.Add(this.chosenIngThings[k].thing);
                doBill.numToBringList.Add(this.chosenIngThings[k].count);
            }
            doBill.haulMode = HaulMode.ToCellNonStorage;
            doBill.bill = bill;
            return doBill;
        }
    }
}
