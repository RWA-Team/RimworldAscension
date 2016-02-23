using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_DoBill : WorkGiver_Scanner
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

        public static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);

        public static string MissingMaterialsTranslated;

        public static string MissingSkillTranslated;

        public static List<Thing> relevantThings = new List<Thing>();

        public static List<Thing> newRelevantThings = new List<Thing>();

        public static List<IngredientCount> ingredientsOrdered = new List<IngredientCount>();

        public static HashSet<Thing> assignedThings = new HashSet<Thing>();

        public static DefCountList availableCounts = new WorkGiver_DoBill.DefCountList();

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (this.def.singleBillGiverDef != null)
                {
                    return ThingRequest.ForDef(this.def.singleBillGiverDef);
                }
                return ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);
            }
        }

        public WorkGiver_DoBill()
        {
            if (WorkGiver_DoBill.MissingSkillTranslated == null)
            {
                WorkGiver_DoBill.MissingSkillTranslated = "MissingSkill".Translate();
            }
            if (WorkGiver_DoBill.MissingMaterialsTranslated == null)
            {
                WorkGiver_DoBill.MissingMaterialsTranslated = "MissingMaterials".Translate();
            }
        }

        public override Job JobOnThing(Pawn pawn, Thing thing)
        {
            IBillGiver billGiver = thing as IBillGiver;
            if (billGiver == null)
            {
                return null;
            }
            if (!this.ThingIsUsableBillGiver(thing))
            {
                return null;
            }
            if (!billGiver.CurrentlyUsable())
            {
                return null;
            }
            if (!billGiver.BillStack.AnyShouldDoNow)
            {
                return null;
            }
            if (!pawn.CanReserve(thing, 1) || thing.IsBurning() || thing.IsForbidden(pawn))
            {
                return null;
            }
            if (!pawn.CanReach(((Thing)billGiver).InteractionCell, PathEndMode.OnCell, Danger.Some, false))
            {
                return null;
            }
            Bill bill = null;
            for (int i = 0; i < billGiver.BillStack.Count; i++)
            {
                Bill bill2 = billGiver.BillStack[i];
                if (Find.TickManager.TicksGame >= bill2.lastIngredientSearchFailTicks + WorkGiver_DoBill.ReCheckFailedBillTicksRange.RandomInRange || FloatMenuMaker.making)
                {
                    if (bill2.ShouldDoNow())
                    {
                        if (bill2.PawnAllowedToStartAnew(pawn))
                        {
                            Bill_ProductionWithUft bill_ProductionWithUft = bill2 as Bill_ProductionWithUft;
                            if (bill_ProductionWithUft != null)
                            {
                                if (bill_ProductionWithUft.BoundUft != null)
                                {
                                    if (bill_ProductionWithUft.BoundWorker != pawn || !pawn.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly, 1))
                                    {
                                        goto IL_1CC;
                                    }
                                    return WorkGiver_DoBill.FinishUftJob(pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft);
                                }
                                else
                                {
                                    UnfinishedThing unfinishedThing = WorkGiver_DoBill.ClosestUnfinishedThingForBill(pawn, bill_ProductionWithUft);
                                    if (unfinishedThing != null)
                                    {
                                        return WorkGiver_DoBill.FinishUftJob(pawn, unfinishedThing, bill_ProductionWithUft);
                                    }
                                }
                            }
                            if (!bill2.recipe.PawnSatisfiesSkillRequirements(pawn))
                            {
                                JobFailReason.Is(WorkGiver_DoBill.MissingSkillTranslated);
                            }
                            else
                            {
                                if (WorkGiver_DoBill.TryFindBestBillIngredients(bill2, pawn, thing, this.chosenIngThings))
                                {
                                    bill = bill2;
                                    break;
                                }
                                if (!FloatMenuMaker.making)
                                {
                                    bill2.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
                                }
                                else
                                {
                                    JobFailReason.Is(WorkGiver_DoBill.MissingMaterialsTranslated);
                                }
                            }
                        }
                    }
                }
            IL_1CC:;
            }
            if (bill == null)
            {
                return null;
            }
            billGiver.BillStack.RemoveInvalidBills();
            bool flag = false;
            for (int j = 0; j < billGiver.BillStack.Count; j++)
            {
                Bill bill3 = billGiver.BillStack[j];
                if (bill3 == bill)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return null;
            }
            Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, billGiver, null);
            if (job != null)
            {
                return job;
            }
            Job job2 = new Job(JobDefOf.DoBill, thing);
            job2.targetQueueB = new List<TargetInfo>(this.chosenIngThings.Count);
            job2.numToBringList = new List<int>(this.chosenIngThings.Count);
            for (int k = 0; k < this.chosenIngThings.Count; k++)
            {
                job2.targetQueueB.Add(this.chosenIngThings[k].thing);
                job2.numToBringList.Add(this.chosenIngThings[k].count);
            }
            job2.haulMode = HaulMode.ToCellNonStorage;
            job2.bill = bill;
            return job2;
        }

        public static UnfinishedThing ClosestUnfinishedThingForBill(Pawn pawn, Bill_ProductionWithUft bill)
        {
            Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn) && ((UnfinishedThing)t).Recipe == bill.recipe && ((UnfinishedThing)t).Creator == pawn && pawn.CanReserve(t, 1);
            Predicate<Thing> validator = predicate;
            return (UnfinishedThing)GenClosest.ClosestThingReachable(pawn.Position, ThingRequest.ForDef(bill.recipe.unfinishedThingDef), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
        }

        public static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
        {
            if (uft.Creator != pawn)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Tried to get FinishUftJob for ",
                    pawn,
                    " finishing ",
                    uft,
                    " but its creator is ",
                    uft.Creator
                }));
                return null;
            }
            Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, bill.billStack.billGiver, uft);
            if (job != null && job.targetA.Thing != uft)
            {
                return job;
            }
            return new Job(JobDefOf.DoBill, (Thing)bill.billStack.billGiver)
            {
                bill = bill,
                targetQueueB = new List<TargetInfo>
                {
                    uft
                },
                numToBringList = new List<int>
                {
                    1
                },
                haulMode = HaulMode.ToCellNonStorage
            };
        }

        public bool ThingIsUsableBillGiver(Thing thing)
        {
            Pawn pawn = thing as Pawn;
            Corpse corpse = thing as Corpse;
            Pawn pawn2 = null;
            if (corpse != null)
            {
                pawn2 = corpse.innerPawn;
            }
            if (thing.def == this.def.singleBillGiverDef || ThingRequestGroup.PotentialBillGiver.Includes(thing.def))
            {
                return true;
            }
            if (pawn != null)
            {
                if (this.def.billGiversAllHumanlikes && pawn.RaceProps.Humanlike)
                {
                    return true;
                }
                if (this.def.billGiversAllMechanoids && pawn.RaceProps.mechanoid)
                {
                    return true;
                }
                if (this.def.billGiversAllAnimals && pawn.RaceProps.Animal)
                {
                    return true;
                }
            }
            if (corpse != null && pawn2 != null)
            {
                if (this.def.billGiversAllHumanlikesCorpses && pawn2.RaceProps.Humanlike)
                {
                    return true;
                }
                if (this.def.billGiversAllMechanoidsCorpses && pawn2.RaceProps.mechanoid)
                {
                    return true;
                }
                if (this.def.billGiversAllAnimalsCorpses && pawn2.RaceProps.Animal)
                {
                    return true;
                }
            }
            return false;
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
            WorkGiver_DoBill.relevantThings.Clear();
            WorkGiver_DoBill.ingredientsOrdered.Clear();
            for (int i = 0; i < bill.recipe.ingredients.Count; i++)
            {
                IngredientCount ingredientCount = bill.recipe.ingredients[i];
                if (ingredientCount.filter.AllowedDefCount == 1)
                {
                    WorkGiver_DoBill.ingredientsOrdered.Add(ingredientCount);
                }
            }
            for (int j = 0; j < bill.recipe.ingredients.Count; j++)
            {
                IngredientCount item = bill.recipe.ingredients[j];
                if (!WorkGiver_DoBill.ingredientsOrdered.Contains(item))
                {
                    WorkGiver_DoBill.ingredientsOrdered.Add(item);
                }
            }
            List<Thing> thingList = null;
            bool foundAll = false;
            Thing t;
            RegionProcessor regionProcessor = delegate (Region r)
            {
                WorkGiver_DoBill.newRelevantThings.Clear();
                thingList = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (int k = 0; k < thingList.Count; k++)
                {
                    t = thingList[k];
                    if (t.SpawnedInWorld && !t.IsForbidden(pawn) && (t.Position - billGiver.Position).LengthHorizontalSquared < bill.ingredientSearchRadius * bill.ingredientSearchRadius && bill.recipe.fixedIngredientFilter.Allows(t) && bill.ingredientFilter.Allows(t) && bill.recipe.ingredients.Any((IngredientCount ingNeed) => ingNeed.filter.Allows(t)) && pawn.CanReserve(t, 1) && (!bill.CheckIngredientsIfSociallyProper || t.IsSociallyProper(pawn)))
                    {
                        WorkGiver_DoBill.newRelevantThings.Add(t);
                    }
                }
                if (WorkGiver_DoBill.newRelevantThings.Count > 0)
                {
                    Comparison<Thing> comparison = delegate (Thing t1, Thing t2)
                    {
                        float lengthHorizontalSquared = (t1.Position - pawn.Position).LengthHorizontalSquared;
                        float lengthHorizontalSquared2 = (t2.Position - pawn.Position).LengthHorizontalSquared;
                        return lengthHorizontalSquared.CompareTo(lengthHorizontalSquared2);
                    };
                    WorkGiver_DoBill.newRelevantThings.Sort(comparison);
                    for (int l = 0; l < WorkGiver_DoBill.newRelevantThings.Count; l++)
                    {
                        WorkGiver_DoBill.relevantThings.Add(WorkGiver_DoBill.newRelevantThings[l]);
                    }
                    WorkGiver_DoBill.newRelevantThings.Clear();
                    if (WorkGiver_DoBill.TryFindBestBillIngredientsInSet(WorkGiver_DoBill.relevantThings, bill, chosen))
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
                return WorkGiver_DoBill.TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen);
            }
            return WorkGiver_DoBill.TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen);
        }

        public static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            RecipeDef recipe = bill.recipe;
            chosen.Clear();
            WorkGiver_DoBill.assignedThings.Clear();
            WorkGiver_DoBill.availableCounts.Clear();
            WorkGiver_DoBill.availableCounts.GenerateFrom(availableThings);
            for (int i = 0; i < WorkGiver_DoBill.ingredientsOrdered.Count; i++)
            {
                IngredientCount ingredientCount = recipe.ingredients[i];
                bool flag = false;
                for (int j = 0; j < WorkGiver_DoBill.availableCounts.Count; j++)
                {
                    float num = (float)ingredientCount.CountRequiredOfFor(WorkGiver_DoBill.availableCounts.GetDef(j), bill.recipe);
                    if (num <= WorkGiver_DoBill.availableCounts.GetCount(j))
                    {
                        if (ingredientCount.filter.Allows(WorkGiver_DoBill.availableCounts.GetDef(j)))
                        {
                            for (int k = 0; k < availableThings.Count; k++)
                            {
                                if (availableThings[k].def == WorkGiver_DoBill.availableCounts.GetDef(j))
                                {
                                    if (!WorkGiver_DoBill.assignedThings.Contains(availableThings[k]))
                                    {
                                        int num2 = Mathf.Min(Mathf.FloorToInt(num), availableThings[k].stackCount);
                                        ThingAmount.AddToList(chosen, availableThings[k], num2);
                                        num -= (float)num2;
                                        WorkGiver_DoBill.assignedThings.Add(availableThings[k]);
                                        if (num < 0.001f)
                                        {
                                            flag = true;
                                            float num3 = WorkGiver_DoBill.availableCounts.GetCount(j);
                                            num3 -= ingredientCount.GetBaseCount();
                                            WorkGiver_DoBill.availableCounts.SetCount(j, num3);
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