using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace RimworldAscension.WorkGivers
{
    public class WorkGiver_DoBill_Research : WorkGiver_Scanner
    {
        public class DefCountList
        {
            public List<ThingDef> defs = new List<ThingDef>();

            public List<float> counts = new List<float>();

            public int Count
            {
                get
                {
                    return this.defs.Count;
                }
            }

            public float this[ThingDef def]
            {
                get
                {
                    int num = this.defs.IndexOf(def);
                    if (num < 0)
                    {
                        return 0f;
                    }
                    return this.counts[num];
                }
                set
                {
                    int num = this.defs.IndexOf(def);
                    if (num < 0)
                    {
                        this.defs.Add(def);
                        this.counts.Add(value);
                        num = this.defs.Count - 1;
                    }
                    else
                    {
                        this.counts[num] = value;
                    }
                    this.CheckRemove(num);
                }
            }

            public float GetCount(int index)
            {
                return this.counts[index];
            }

            public void SetCount(int index, float val)
            {
                this.counts[index] = val;
                this.CheckRemove(index);
            }

            public ThingDef GetDef(int index)
            {
                return this.defs[index];
            }

            public void CheckRemove(int index)
            {
                if (this.counts[index] == 0f)
                {
                    this.counts.RemoveAt(index);
                    this.defs.RemoveAt(index);
                }
            }

            public void Clear()
            {
                this.defs.Clear();
                this.counts.Clear();
            }

            public void GenerateFrom(List<Thing> things)
            {
                this.Clear();
                for (int i = 0; i < things.Count; i++)
                {
                    ThingDef def;
                    ThingDef expr_1C = def = things[i].def;
                    float num = this[def];
                    this[expr_1C] = num + (float)things[i].stackCount;
                }
            }
        }

        public List<ThingAmount> chosenIngThings = new List<ThingAmount>();
        
        public static string MissingMaterialsTranslated;

        public static List<Thing> relevantThings = new List<Thing>();

        public static List<Thing> newRelevantThings = new List<Thing>();

        public static List<IngredientCount> ingredientsOrdered = new List<IngredientCount>();

        public static HashSet<Thing> assignedThings = new HashSet<Thing>();

        public static WorkGiver_DoBill_Research.DefCountList availableCounts = new WorkGiver_DoBill_Research.DefCountList();

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

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
            return t.def == ThingDefOf.ResearchBench && pawn.CanReserve(t, 1);
        }

        public WorkGiver_DoBill_Research()
        {
            if (WorkGiver_DoBill_Research.MissingMaterialsTranslated == null)
            {
                WorkGiver_DoBill_Research.MissingMaterialsTranslated = "MissingMaterials".Translate();
            }
        }

        public override Job JobOnThing(Pawn pawn, Thing researchBench)
        {
            IBillGiver billGiver = researchBench as IBillGiver;
            Bill bill = null;
            Dictionary<ResearchProjectDef, RecipeDef> researchRecipes = researchBench.TryGetComp<RimworldAscension.CompResearcher>().Properties.researchRecipes;

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

            Log.Message("pawn = " + pawn);
            Log.Message("bench = " + researchBench);


            // researchBench has added bills
            if (billGiver.BillStack.Count == 1)
            {
                // research project is finished or changed
                if (researchRecipes.Any(pair => pair.Value == billGiver.BillStack[0].recipe && (pair.Key.IsFinished || pair.Key != Find.ResearchManager.currentProj)))
                {
                    Log.Message("cleared");
                    billGiver.BillStack.Clear();
                }
            }

            Log.Message("billstack.count = " + billGiver.BillStack.Count);

            // Add research bill if it's not added already
            if (billGiver.BillStack.Count == 0)
            {
                bill = new Bill_Production(researchRecipes[Find.ResearchManager.currentProj]);
                bill.suspended = true;
                billGiver.BillStack.AddBill(bill);
                Log.Message("added new bill");
            }
            else
            {
                Log.Message("research = " + Find.ResearchManager.currentProj);
                bill = billGiver.BillStack[0];
            }

            if (!WorkGiver_DoBill_Research.TryFindBestBillIngredients(bill, pawn, researchBench, this.chosenIngThings))
            {
                Log.Message("no materials");
                if (FloatMenuMaker.making)
                {
                    JobFailReason.Is(WorkGiver_DoBill_Research.MissingMaterialsTranslated);
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
                Log.Message("haul aside");
                return haulAside;
            }

            Log.Message("do bill");
            // gather ingridients and do bill
            Job doBill = new Job(JobDefOf.Research, researchBench);
            doBill.targetQueueB = new List<TargetInfo>(this.chosenIngThings.Count);
            doBill.numToBringList = new List<int>(this.chosenIngThings.Count);
            for (int k = 0; k < this.chosenIngThings.Count; k++)
            {
                Log.Message("item = " + this.chosenIngThings[k].thing);
                // pre reservation is made to assure that current researcher is cleaning the bench, not some other pawn with research skill
                ReservationUtility.Reserve(pawn, this.chosenIngThings[k].thing, 1);
                doBill.targetQueueB.Add(this.chosenIngThings[k].thing);
                doBill.numToBringList.Add(this.chosenIngThings[k].count);
            }
            doBill.haulMode = HaulMode.ToCellNonStorage;
            doBill.bill = bill;
            return doBill;
        }


        public static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingAmount> chosen)
        {
            if (bill.recipe.ingredients.Count == 0)
            {
                chosen.Clear();
                return true;
            }
            Building building = billGiver as Building;
            IntVec3 c;
            if (building != null)
            {
                if (building.def.hasInteractionCell)
                {
                    c = building.InteractionCell;
                }
                else
                {
                    c = pawn.Position;
                    Log.Error("Tried to find bill ingredients for " + billGiver + " which has no interaction cell.");
                }
            }
            else
            {
                c = billGiver.Position;
            }
            Region validRegionAt = Find.RegionGrid.GetValidRegionAt(c);
            if (validRegionAt == null)
            {
                return false;
            }
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            RegionEntryPredicate entryCondition = (Region r) => r.Allows(traverseParams, false);
            WorkGiver_DoBill_Research.relevantThings.Clear();
            WorkGiver_DoBill_Research.ingredientsOrdered.Clear();
            for (int i = 0; i < bill.recipe.ingredients.Count; i++)
            {
                IngredientCount ingredientCount = bill.recipe.ingredients[i];
                if (ingredientCount.filter.AllowedDefCount == 1)
                {
                    WorkGiver_DoBill_Research.ingredientsOrdered.Add(ingredientCount);
                }
            }
            for (int j = 0; j < bill.recipe.ingredients.Count; j++)
            {
                IngredientCount item = bill.recipe.ingredients[j];
                if (!WorkGiver_DoBill_Research.ingredientsOrdered.Contains(item))
                {
                    WorkGiver_DoBill_Research.ingredientsOrdered.Add(item);
                }
            }
            List<Thing> thingList = null;
            bool foundAll = false;
            Thing t;
            RegionProcessor regionProcessor = delegate(Region r)
            {
                WorkGiver_DoBill_Research.newRelevantThings.Clear();
                thingList = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (int k = 0; k < thingList.Count; k++)
                {
                    t = thingList[k];
                    if (t.SpawnedInWorld && !t.IsForbidden(pawn) && (t.Position - billGiver.Position).LengthHorizontalSquared < bill.ingredientSearchRadius * bill.ingredientSearchRadius && bill.recipe.fixedIngredientFilter.Allows(t) && bill.ingredientFilter.Allows(t) && bill.recipe.ingredients.Any((IngredientCount ingNeed) => ingNeed.filter.Allows(t)) && pawn.CanReserve(t, 1) && (!bill.CheckIngredientsIfSociallyProper || t.IsSociallyProper(pawn)))
                    {
                        WorkGiver_DoBill_Research.newRelevantThings.Add(t);
                    }
                }
                if (WorkGiver_DoBill_Research.newRelevantThings.Count > 0)
                {
                    Comparison<Thing> comparison = delegate(Thing t1, Thing t2)
                    {
                        float lengthHorizontalSquared = (t1.Position - pawn.Position).LengthHorizontalSquared;
                        float lengthHorizontalSquared2 = (t2.Position - pawn.Position).LengthHorizontalSquared;
                        return lengthHorizontalSquared.CompareTo(lengthHorizontalSquared2);
                    };
                    WorkGiver_DoBill_Research.newRelevantThings.Sort(comparison);
                    for (int l = 0; l < WorkGiver_DoBill_Research.newRelevantThings.Count; l++)
                    {
                        WorkGiver_DoBill_Research.relevantThings.Add(WorkGiver_DoBill_Research.newRelevantThings[l]);
                    }
                    WorkGiver_DoBill_Research.newRelevantThings.Clear();
                    if (WorkGiver_DoBill_Research.TryFindBestBillIngredientsInSet(WorkGiver_DoBill_Research.relevantThings, bill, chosen))
                    {
                        foundAll = true;
                        return true;
                    }
                }
                return false;
            };
            RegionTraverser.BreadthFirstTraverse(validRegionAt, entryCondition, regionProcessor, 99999);
            return foundAll;
        }

        public static bool TryFindBestBillIngredientsInSet(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            if (bill.recipe.allowMixingIngredients)
            {
                return WorkGiver_DoBill_Research.TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen);
            }
            return WorkGiver_DoBill_Research.TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen);
        }

        public static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            RecipeDef recipe = bill.recipe;
            chosen.Clear();
            WorkGiver_DoBill_Research.assignedThings.Clear();
            WorkGiver_DoBill_Research.availableCounts.Clear();
            WorkGiver_DoBill_Research.availableCounts.GenerateFrom(availableThings);
            for (int i = 0; i < WorkGiver_DoBill_Research.ingredientsOrdered.Count; i++)
            {
                IngredientCount ingredientCount = recipe.ingredients[i];
                bool flag = false;
                for (int j = 0; j < WorkGiver_DoBill_Research.availableCounts.Count; j++)
                {
                    float num = (float)ingredientCount.CountRequiredOfFor(WorkGiver_DoBill_Research.availableCounts.GetDef(j), bill.recipe);
                    if (num <= WorkGiver_DoBill_Research.availableCounts.GetCount(j))
                    {
                        if (ingredientCount.filter.Allows(WorkGiver_DoBill_Research.availableCounts.GetDef(j)))
                        {
                            for (int k = 0; k < availableThings.Count; k++)
                            {
                                if (availableThings[k].def == WorkGiver_DoBill_Research.availableCounts.GetDef(j))
                                {
                                    if (!WorkGiver_DoBill_Research.assignedThings.Contains(availableThings[k]))
                                    {
                                        int num2 = Mathf.Min(Mathf.FloorToInt(num), availableThings[k].stackCount);
                                        ThingAmount.AddToList(chosen, availableThings[k], num2);
                                        num -= (float)num2;
                                        WorkGiver_DoBill_Research.assignedThings.Add(availableThings[k]);
                                        if (num < 0.001f)
                                        {
                                            flag = true;
                                            float num3 = WorkGiver_DoBill_Research.availableCounts.GetCount(j);
                                            num3 -= ingredientCount.GetBaseCount();
                                            WorkGiver_DoBill_Research.availableCounts.SetCount(j, num3);
                                            break;
                                        }
                                    }
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool TryFindBestBillIngredientsInSet_AllowMix(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            chosen.Clear();
            for (int i = 0; i < bill.recipe.ingredients.Count; i++)
            {
                IngredientCount ingredientCount = bill.recipe.ingredients[i];
                float num = ingredientCount.GetBaseCount();
                for (int j = 0; j < availableThings.Count; j++)
                {
                    Thing thing = availableThings[j];
                    if (ingredientCount.filter.Allows(thing))
                    {
                        float num2 = bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                        int num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
                        ThingAmount.AddToList(chosen, thing, num3);
                        num -= (float)num3 * num2;
                        if (num <= 0.0001f)
                        {
                            break;
                        }
                    }
                }
                if (num > 0.0001f)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
