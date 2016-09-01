using System;
using System.Collections.Generic;
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

            public int Count => defs.Count;

            public float this[ThingDef def]
            {
                get
                {
                    var num = defs.IndexOf(def);
                    if (num < 0)
                    {
                        return 0f;
                    }
                    return counts[num];
                }
                set
                {
                    var num = defs.IndexOf(def);
                    if (num < 0)
                    {
                        defs.Add(def);
                        counts.Add(value);
                        num = defs.Count - 1;
                    }
                    else
                    {
                        counts[num] = value;
                    }
                    CheckRemove(num);
                }
            }

            public float GetCount(int index)
            {
                return counts[index];
            }

            public void SetCount(int index, float val)
            {
                counts[index] = val;
                CheckRemove(index);
            }

            public ThingDef GetDef(int index)
            {
                return defs[index];
            }

            public void CheckRemove(int index)
            {
                if (counts[index] == 0f)
                {
                    counts.RemoveAt(index);
                    defs.RemoveAt(index);
                }
            }

            public void Clear()
            {
                defs.Clear();
                counts.Clear();
            }

            public void GenerateFrom(List<Thing> things)
            {
                Clear();
                foreach (var thing in things)
                {
                    ThingDef def;
                    var expr_1C = def = thing.def;
                    var num = this[def];
                    this[expr_1C] = num + thing.stackCount;
                }
            }
        }

        public List<ThingAmount> chosenIngridiens = new List<ThingAmount>();

        public static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);

        public static string MissingMaterialsTranslated;

        public static string MissingSkillTranslated;

        public static List<Thing> relevantThings = new List<Thing>();

        public static List<Thing> newRelevantThings = new List<Thing>();

        public static List<IngredientCount> ingredientsOrdered = new List<IngredientCount>();

        public static HashSet<Thing> assignedThings = new HashSet<Thing>();

        public static DefCountList availableCounts = new DefCountList();

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (def.fixedBillGiverDefs != null && def.fixedBillGiverDefs.Count == 1)
                {
                    return ThingRequest.ForDef(def.fixedBillGiverDefs[0]);
                }
                return ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);
            }
        }

        public WorkGiver_DoBill()
        {
            if (MissingSkillTranslated == null)
            {
                MissingSkillTranslated = "MissingSkill".Translate();
            }
            if (MissingMaterialsTranslated == null)
            {
                MissingMaterialsTranslated = "MissingMaterials".Translate();
            }
        }

        public override Job JobOnThing(Pawn pawn, Thing thing)
        {
            var billGiver = thing as IBillGiver;
            if (billGiver == null)
            {
                return null;
            }
            if (!ThingIsUsableBillGiver(thing))
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
            if (!pawn.CanReserve(thing) || thing.IsBurning() || thing.IsForbidden(pawn))
            {
                return null;
            }
            if (!pawn.CanReach(((Thing)billGiver).InteractionCell, PathEndMode.OnCell, Danger.Some))
            {
                return null;
            }
            Bill bill = null;
            foreach (var bill2 in billGiver.BillStack)
            {
                if (Find.TickManager.TicksGame >= bill2.lastIngredientSearchFailTicks + ReCheckFailedBillTicksRange.RandomInRange || FloatMenuMakerMap.making)
                {
                    if (bill2.ShouldDoNow())
                    {
                        if (bill2.PawnAllowedToStartAnew(pawn))
                        {
                            var bill_ProductionWithUft = bill2 as Bill_ProductionWithUft;
                            if (bill_ProductionWithUft != null)
                            {
                                if (bill_ProductionWithUft.BoundUft != null)
                                {
                                    if (bill_ProductionWithUft.BoundWorker != pawn || !pawn.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly))
                                    {
                                        goto IL_1CC;
                                    }
                                    return FinishUftJob(pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft);
                                }
                                var unfinishedThing = ClosestUnfinishedThingForBill(pawn, bill_ProductionWithUft);
                                if (unfinishedThing != null)
                                {
                                    return FinishUftJob(pawn, unfinishedThing, bill_ProductionWithUft);
                                }
                            }
                            if (!bill2.recipe.PawnSatisfiesSkillRequirements(pawn))
                            {
                                JobFailReason.Is(MissingSkillTranslated);
                            }
                            else
                            {
                                if (TryFindBestBillIngredients(bill2, pawn, thing, chosenIngridiens))
                                {
                                    bill = bill2;
                                    break;
                                }
                                if (!FloatMenuMakerMap.making)
                                {
                                    bill2.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
                                }
                                else
                                {
                                    JobFailReason.Is(MissingMaterialsTranslated);
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
            var flag = false;
            foreach (var bill3 in billGiver.BillStack)
            {
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
            var job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, billGiver, null);
            if (job != null)
            {
                return job;
            }
            var job2 = new Job(JobDefOf.DoBill, thing)
            {
                targetQueueB = new List<TargetInfo>(chosenIngridiens.Count),
                numToBringList = new List<int>(chosenIngridiens.Count)
            };
            for (var k = 0; k < chosenIngridiens.Count; k++)
            {
                job2.targetQueueB.Add(chosenIngridiens[k].thing);
                job2.numToBringList.Add(chosenIngridiens[k].count);
            }
            job2.haulMode = HaulMode.ToCellNonStorage;
            job2.bill = bill;
            return job2;
        }

        public static UnfinishedThing ClosestUnfinishedThingForBill(Pawn pawn, Bill_ProductionWithUft bill)
        {
            Predicate<Thing> predicate = t => !t.IsForbidden(pawn) && ((UnfinishedThing)t).Recipe == bill.recipe && ((UnfinishedThing)t).Creator == pawn && pawn.CanReserve(t);
            var validator = predicate;
            return (UnfinishedThing)GenClosest.ClosestThingReachable(pawn.Position, ThingRequest.ForDef(bill.recipe.unfinishedThingDef), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999f, validator);
        }

        public static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
        {
            if (uft.Creator != pawn)
            {
                Log.Error(string.Concat("Tried to get FinishUftJob for ", pawn, " finishing ", uft, " but its creator is ", uft.Creator));
                return null;
            }
            var job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, bill.billStack.billGiver, uft);
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
            var pawn = thing as Pawn;
            var corpse = thing as Corpse;
            Pawn pawn2 = null;
            if (corpse != null)
            {
                pawn2 = corpse.innerPawn;
            }
            if (def.fixedBillGiverDefs?.Contains(thing.def) ?? false)
            {
                return true;
            }
            if (pawn != null)
            {
                if (def.billGiversAllHumanlikes && pawn.RaceProps.Humanlike)
                {
                    return true;
                }
                if (def.billGiversAllMechanoids && pawn.RaceProps.IsMechanoid)
                {
                    return true;
                }
                if (def.billGiversAllAnimals && pawn.RaceProps.Animal)
                {
                    return true;
                }
            }
            if (corpse != null && pawn2 != null)
            {
                if (def.billGiversAllHumanlikesCorpses && pawn2.RaceProps.Humanlike)
                {
                    return true;
                }
                if (def.billGiversAllMechanoidsCorpses && pawn2.RaceProps.IsMechanoid)
                {
                    return true;
                }
                if (def.billGiversAllAnimalsCorpses && pawn2.RaceProps.Animal)
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
            var building = billGiver as Building;
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
            var validRegionAt = Find.RegionGrid.GetValidRegionAt(c);
            if (validRegionAt == null)
            {
                return false;
            }
            var traverseParams = TraverseParms.For(pawn);
            RegionEntryPredicate entryCondition = (@from, r) => r.Allows(traverseParams, false);
            relevantThings.Clear();
            ingredientsOrdered.Clear();
            foreach (var ingredientCount in bill.recipe.ingredients)
            {
                if (ingredientCount.filter.AllowedDefCount == 1)
                {
                    ingredientsOrdered.Add(ingredientCount);
                }
            }
            foreach (var item in bill.recipe.ingredients)
            {
                if (!ingredientsOrdered.Contains(item))
                {
                    ingredientsOrdered.Add(item);
                }
            }
            List<Thing> thingList;
            var foundAll = false;
            RegionProcessor regionProcessor = delegate (Region r)
            {
                newRelevantThings.Clear();
                thingList = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                foreach (var thing in thingList)
                {
                    if (thing.Spawned && !thing.IsForbidden(pawn) && (thing.Position - billGiver.Position).LengthHorizontalSquared < bill.ingredientSearchRadius * bill.ingredientSearchRadius && bill.recipe.fixedIngredientFilter.Allows(thing) && bill.ingredientFilter.Allows(thing) && bill.recipe.ingredients.Any(ingNeed => ingNeed.filter.Allows(thing)) && pawn.CanReserve(thing) && (!bill.CheckIngredientsIfSociallyProper || thing.IsSociallyProper(pawn)))
                    {
                        newRelevantThings.Add(thing);
                    }
                }
                if (newRelevantThings.Count > 0)
                {
                    Comparison<Thing> comparison = delegate (Thing t1, Thing t2)
                    {
                        var lengthHorizontalSquared = (t1.Position - pawn.Position).LengthHorizontalSquared;
                        var lengthHorizontalSquared2 = (t2.Position - pawn.Position).LengthHorizontalSquared;
                        return lengthHorizontalSquared.CompareTo(lengthHorizontalSquared2);
                    };
                    newRelevantThings.Sort(comparison);
                    foreach (var newThing in newRelevantThings)
                    {
                        relevantThings.Add(newThing);
                    }
                    newRelevantThings.Clear();
                    if (TryFindBestBillIngredientsInSet(relevantThings, bill, chosen))
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
                return TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen);
            }
            return TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen);
        }

        public static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            var recipe = bill.recipe;
            chosen.Clear();
            assignedThings.Clear();
            availableCounts.Clear();
            availableCounts.GenerateFrom(availableThings);
            for (var i = 0; i < ingredientsOrdered.Count; i++)
            {
                var ingredientCount = recipe.ingredients[i];
                var flag = false;
                for (var j = 0; j < availableCounts.Count; j++)
                {
                    float num = ingredientCount.CountRequiredOfFor(availableCounts.GetDef(j), bill.recipe);
                    if (num <= availableCounts.GetCount(j))
                    {
                        if (ingredientCount.filter.Allows(availableCounts.GetDef(j)))
                        {
                            foreach (var thing in availableThings)
                            {
                                if (thing.def == availableCounts.GetDef(j))
                                {
                                    if (!assignedThings.Contains(thing))
                                    {
                                        var num2 = Mathf.Min(Mathf.FloorToInt(num), thing.stackCount);
                                        ThingAmount.AddToList(chosen, thing, num2);
                                        num -= num2;
                                        assignedThings.Add(thing);
                                        if (num < 0.001f)
                                        {
                                            flag = true;
                                            var num3 = availableCounts.GetCount(j);
                                            num3 -= ingredientCount.GetBaseCount();
                                            availableCounts.SetCount(j, num3);
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
            foreach (var ingredientCount in bill.recipe.ingredients)
            {
                var num = ingredientCount.GetBaseCount();
                foreach (var thing in availableThings)
                {
                    if (ingredientCount.filter.Allows(thing))
                    {
                        var num2 = bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                        var num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
                        ThingAmount.AddToList(chosen, thing, num3);
                        num -= num3 * num2;
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