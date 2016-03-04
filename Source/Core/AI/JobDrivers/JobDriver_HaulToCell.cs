using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RA
{
    public class JobDriver_HaulToCell : JobDriver
    {
        public const TargetIndex HaulableInd = TargetIndex.A;
        public const TargetIndex StoreCellInd = TargetIndex.B;

        // duplicated cause vanilla is private
        public static readonly int PlaceNearMiddleRadialCells = GenRadial.NumCellsInRadius(3f);
        public static readonly int PlaceNearMaxRadialCells = GenRadial.NumCellsInRadius(12.9f);

        // duplicated cause vanilla is private
        public enum PlaceSpotQuality : byte
        {
            Unusable,
            Awful,
            Bad,
            Okay,
            Perfect
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Set fail conditions
            this.FailOnDestroyed(HaulableInd);
            this.FailOnBurningImmobile(StoreCellInd);
            //Note we only fail on forbidden if the target doesn't start that way
            //actor.carrier helps haul-aside jobs on forbidden items
            if (!TargetThingA.IsForbidden(pawn))
                this.FailOnForbidden(HaulableInd);

            //Reserve target storage cell
            yield return Toils_Reserve.Reserve(StoreCellInd);

            //Reserve thing to be stored
            var reserveTargetA = Toils_Reserve.Reserve(HaulableInd);
            yield return reserveTargetA;

            Toil toilGoto = null;
            toilGoto = Toils_Goto.GotoThing(HaulableInd, PathEndMode.ClosestTouch)
                .FailOn(() =>
                {
                    //Note we don't fail on losing hauling designation
                    //Because that's a special case anyway

                    //While hauling to cell storage, ensure storage dest is still valid
                    var actor = toilGoto.actor;
                    var curJob = actor.jobs.curJob;
                    if (curJob.haulMode == HaulMode.ToCellStorage)
                    {
                        var haulThing = curJob.GetTarget(HaulableInd).Thing;

                        var destLoc = actor.jobs.curJob.GetTarget(TargetIndex.B).Cell;
                        if (!IsValidStorageFor(destLoc, haulThing))
                            return true;
                    }

                    return false;
                });
            yield return toilGoto;


            yield return Toils_Haul.StartCarryThing(HaulableInd);

            // duplicated to make changes
            if (CurJob.haulOpportunisticDuplicates)
                yield return CheckForGetOpportunityDuplicate(reserveTargetA, HaulableInd, StoreCellInd);

            // duplicated to make changes
            var carryToCell = CarryHauledThingToCell(StoreCellInd);
            yield return carryToCell;

            // duplicated to make changes
            yield return PlaceHauledThingInCell(StoreCellInd, carryToCell, true);
        }

        // duplicated to make changes
        public Toil CheckForGetOpportunityDuplicate(Toil getHaulTargetToil, TargetIndex haulableInd, TargetIndex storeCellInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;

                if (actor.carrier.CarriedThing.def.stackLimit == 1)
                {
                    return;
                }
                if (actor.carrier.Full)
                {
                    return;
                }
                var num = curJob.maxNumToCarry - actor.carrier.CarriedThing.stackCount;
                if (num <= 0)
                {
                    return;
                }
                // call duplicated to make changes (IsValidStorageFor)
                // NOTE !t.IsInValidStorage() might require optimization
                Predicate<Thing> validator = t => t.SpawnedInWorld && t.def == actor.carrier.CarriedThing.def && !t.IsForbidden(actor) && !t.IsInValidStorage() && (storeCellInd == TargetIndex.None || IsValidStorageFor(curJob.GetTarget(storeCellInd).Cell , t)) && actor.CanReserve(t);

                var thing = GenClosest.ClosestThingReachable(actor.Position, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.ClosestTouch, TraverseParms.For(actor), 8f, validator);

                if (thing != null)
                {
                    curJob.SetTarget(haulableInd, thing);
                    actor.jobs.curDriver.SetNextToil(getHaulTargetToil);
                    actor.jobs.curDriver.SetCompleteMode(ToilCompleteMode.Instant);
                }
            };
            return toil;
        }

        // duplicated to make changes
        public Toil PlaceHauledThingInCell(TargetIndex cellInd, Toil nextToilOnPlaceFailOrIncomplete, bool storageMode)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var cell = curJob.GetTarget(cellInd).Cell;
                var slotGroup = Find.SlotGroupManager.SlotGroupAt(cell);
                if (slotGroup != null && slotGroup.Settings.AllowedToAccept(actor.carrier.CarriedThing))
                {
                    Find.DesignationManager.RemoveAllDesignationsOn(actor.carrier.CarriedThing);
                }
                Thing thing;
                // call duplicated to make changes
                if (TryDropCarriedThing(actor, cell, ThingPlaceMode.Direct, out thing))
                {
                    if (curJob.def == JobDefOf.DoBill && thing != null)
                    {
                        if (curJob.placedTargets == null)
                        {
                            curJob.placedTargets = new List<TargetInfo>();
                        }
                        if (!curJob.placedTargets.Contains(thing))
                        {
                            curJob.placedTargets.Add(thing);
                        }
                    }
                }
                else if (storageMode)
                {
                    IntVec3 vec;
                    // call duplicated to make changes
                    if (nextToilOnPlaceFailOrIncomplete != null && WorkGiver_HaulGeneral.TryFindBestBetterStoreCellFor(actor.carrier.CarriedThing, actor, StoragePriority.Unstored, actor.Faction, out vec))
                    {
                        actor.CurJob.SetTarget(cellInd, vec);
                        actor.jobs.curDriver.SetNextToil(nextToilOnPlaceFailOrIncomplete);
                        return;
                    }
                    var job = HaulAIUtility.HaulAsideJobFor(actor, actor.carrier.CarriedThing);
                    if (job != null)
                    {
                        curJob.targetA = job.targetA;
                        curJob.targetB = job.targetB;
                        curJob.targetC = job.targetC;
                        curJob.maxNumToCarry = job.maxNumToCarry;
                        curJob.haulOpportunisticDuplicates = job.haulOpportunisticDuplicates;
                        curJob.haulMode = job.haulMode;
                        actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
                    }
                    else
                    {
                        Log.Error(string.Concat("Incomplete haul for ", actor, ": Could not find anywhere to put ", actor.carrier.CarriedThing, " near ", actor.Position, ". Destroying. actor.carrier should never happen!"));
                        actor.carrier.CarriedThing.Destroy();
                    }
                }
                else if (nextToilOnPlaceFailOrIncomplete != null)
                {
                    actor.jobs.curDriver.SetNextToil(nextToilOnPlaceFailOrIncomplete);
                }
            };
            return toil;
        }

        // duplicated to make changes
        public Toil CarryHauledThingToCell(TargetIndex squareIndex)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var cell = toil.actor.jobs.curJob.GetTarget(squareIndex).Cell;
                toil.actor.pather.StartPath(cell, PathEndMode.ClosestTouch);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.AddFailCondition(delegate
            {
                var actor = toil.actor;
                var cell = actor.jobs.curJob.GetTarget(squareIndex).Cell;

                // call duplicated to make changes
                return actor.jobs.curJob.haulMode == HaulMode.ToCellStorage && !IsValidStorageFor(cell, actor.carrier.CarriedThing);
            });
            return toil;
        }
        
        // duplicated to make changes
        public bool IsValidStorageFor(IntVec3 c, Thing storable)
        {
            // call duplicated to make changes
            if (!WorkGiver_HaulGeneral.NoStorageBlockersIn(c, storable))
            {
                return false;
            }
            var slotGroup = c.GetSlotGroup();
            return slotGroup != null && slotGroup.Settings.AllowedToAccept(storable);
        }

        // duplicated to make changes
        public bool TryDropCarriedThing(Pawn actor, IntVec3 dropLoc, ThingPlaceMode mode, out Thing resultingThing)
        {
            // call duplicated to make changes
            if (TryDrop(actor, actor.carrier.CarriedThing, dropLoc, mode, out resultingThing))
            {
                if (actor.Faction.HostileTo(Faction.OfColony))
                {
                    resultingThing.SetForbidden(true, false);
                }
                return true;
            }
            return false;
        }

        // duplicated to make changes
        public bool TryDrop(Pawn actor, Thing thing, IntVec3 dropLoc, ThingPlaceMode mode, out Thing lastResultingThing)
        {
            if (!actor.carrier.container.Contains(thing))
            {
                Log.Error(string.Concat(actor.carrier.container.owner, " container tried to drop  ", thing, " which it didn't contain."));
                lastResultingThing = null;
                return false;
            }
            // call duplicated to make changes
            if (TryDropSpawn(thing, dropLoc, mode, out lastResultingThing))
            {
                actor.carrier.container.Remove(thing);
                return true;
            }
            return false;
        }

        // duplicated to make changes
        public bool TryDropSpawn(Thing thing, IntVec3 dropCell, ThingPlaceMode mode, out Thing resultingThing)
        {
            if (!dropCell.InBounds())
            {
                Log.Error(string.Concat("Dropped ", thing, " out of bounds at ", dropCell));
                resultingThing = null;
                return false;
            }
            if (thing.def.destroyOnDrop)
            {
                thing.Destroy();
                resultingThing = null;
                return true;
            }
            thing.def.soundDrop?.PlayOneShot(dropCell);
            // call duplicated to make changes
            return TryPlaceThing(thing, dropCell, mode, out resultingThing);
        }

        // duplicated to make changes
        public bool TryPlaceThing(Thing thing, IntVec3 center, ThingPlaceMode mode, out Thing lastResultingThing)
        {
            if (thing.def.category == ThingCategory.Filth)
            {
                mode = ThingPlaceMode.Direct;
            }
            if (mode == ThingPlaceMode.Direct)
            {
                // call duplicated to make changes
                return TryPlaceDirect(thing, center, out lastResultingThing);
            }
            if (mode == ThingPlaceMode.Near)
            {
                lastResultingThing = null;
                while (true)
                {
                    var stackCount = thing.stackCount;
                    IntVec3 loc;
                    // call duplicated cause vanilla is private
                    if (!TryFindPlaceSpotNear(center, thing, out loc))
                    {
                        break;
                    }
                    // call duplicated to make changes
                    if (TryPlaceDirect(thing, loc, out lastResultingThing))
                    {
                        return true;
                    }
                    if (thing.stackCount == stackCount)
                    {
                        goto Block_6;
                    }
                }
                return false;
                Block_6:
                Log.Error(string.Concat("Failed to place ", thing, " at ", center, " in mode ", mode, "."));
                lastResultingThing = null;
                return false;
            }
            throw new InvalidOperationException();
        }
        
        // duplicated to make changes (added container support)
        public bool TryPlaceDirect(Thing thing, IntVec3 loc, out Thing resultingThing)
        {
            // boolean success indicator
            var flag = false;

            // container code part
            var list = Find.ThingGrid.ThingsListAt(loc);
            if (list.Exists(storage => storage.TryGetComp<CompContainer>() != null))
            {
                // stackables (like resources)
                if (thing.def.stackLimit > 1)
                {
                    foreach (var item in list)

                    {
                        if (!item.CanStackWith(thing))
                        {
                            continue;
                        }
                        // if item can stack with another
                        // Required, because thing reference is changed to the absorber, if absorbed
                        if (item.TryAbsorbStack(thing, true))
                        {
                            // it gets absorbed and destroyed
                            resultingThing = item;
                            return !flag;
                        }
                    }
                }

                resultingThing = GenSpawn.Spawn(thing, loc);

                return !flag;
            }

            // vanilla code part
            if (thing.stackCount > thing.def.stackLimit)
            {
                thing = thing.SplitOff(thing.def.stackLimit);
                flag = true;
            }
            if (thing.def.stackLimit > 1)
            {
                var thingList = loc.GetThingList();
                var i = 0;
                while (i < thingList.Count)
                {
                    var thing2 = thingList[i];
                    if (!thing2.CanStackWith(thing))
                    {
                        i++;
                    }
                    else
                    {
                        // Required, because thing reference is changed to the absorber, if absorbed
                        if (thing2.TryAbsorbStack(thing, true))
                        {
                            resultingThing = thing2;
                            return !flag;
                        }
                        resultingThing = null;
                        return false;
                    }
                }
            }

            resultingThing = GenSpawn.Spawn(thing, loc);

            var slotGroup1 = loc.GetSlotGroup();
            slotGroup1?.parent?.Notify_ReceivedThing(resultingThing);
            return !flag;
        }

        // duplicated to make changes
        public static bool TryFindPlaceSpotNear(IntVec3 center, Thing thing, out IntVec3 bestSpot)
        {
            var placeSpotQuality = PlaceSpotQuality.Unusable;
            bestSpot = center;
            // try place in 8 adjacent cells
            for (var i = 0; i < 9; i++)
            {
                var intVec = center + GenRadial.RadialPattern[i];
                // call duplicated cause vanilla is private
                var placeSpotQuality2 = PlaceSpotQualityAt(intVec, thing, center);
                if (placeSpotQuality2 > placeSpotQuality)
                {
                    bestSpot = intVec;
                    placeSpotQuality = placeSpotQuality2;
                }
                if (placeSpotQuality == PlaceSpotQuality.Perfect)
                {
                    break;
                }
            }
            if (placeSpotQuality >= PlaceSpotQuality.Okay)
            {
                return true;
            }
            for (var j = 0; j < PlaceNearMiddleRadialCells; j++)
            {
                var intVec = center + GenRadial.RadialPattern[j];
                // call duplicated cause vanilla is private
                var placeSpotQuality2 = PlaceSpotQualityAt(intVec, thing, center);
                if (placeSpotQuality2 > placeSpotQuality)
                {
                    bestSpot = intVec;
                    placeSpotQuality = placeSpotQuality2;
                }
                if (placeSpotQuality == PlaceSpotQuality.Perfect)
                {
                    break;
                }
            }
            if (placeSpotQuality >= PlaceSpotQuality.Okay)
            {
                return true;
            }
            for (var k = 0; k < PlaceNearMaxRadialCells; k++)
            {
                var intVec = center + GenRadial.RadialPattern[k];
                // call duplicated cause vanilla is private
                var placeSpotQuality2 = PlaceSpotQualityAt(intVec, thing, center);
                if (placeSpotQuality2 > placeSpotQuality)
                {
                    bestSpot = intVec;
                    placeSpotQuality = placeSpotQuality2;
                }
                if (placeSpotQuality == PlaceSpotQuality.Perfect)
                {
                    break;
                }
            }
            if (placeSpotQuality > PlaceSpotQuality.Unusable)
            {
                return true;
            }
            bestSpot = center;
            return false;
        }

        // overhauled version (vanilla is bugged)
        public static PlaceSpotQuality PlaceSpotQualityAt(IntVec3 c, Thing thing, IntVec3 center)
        {
            if (!c.InBounds() || !c.Walkable())
            {
                return PlaceSpotQuality.Unusable;
            }

            var list = Find.ThingGrid.ThingsListAt(c);

            // if other things on cell
            var i = 0;
            while (i < list.Count)
            {
                var thing2 = list[i];
                if (thing.def.saveCompressible && thing2.def.saveCompressible)
                {
                    return PlaceSpotQuality.Unusable;
                }
                // same thing type
                if (thing2.def.category == ThingCategory.Item)
                {
                    // can stack with
                    if (thing2.def == thing.def && thing2.stackCount < thing.def.stackLimit)
                    {
                        // can absorb
                        // Required, because thing reference is changed to the absorber, if absorbed
                        if (thing.TryAbsorbStack(thing2, true))
                        {
                            return PlaceSpotQuality.Perfect;
                        }
                        // cannot absorb all/anything
                        return PlaceSpotQuality.Unusable;
                    }
                    return PlaceSpotQuality.Unusable;
                }
                i++;
            }
            // if in same room
            if (c.GetRoom() == center.GetRoom())
            {
                var placeSpotQuality = PlaceSpotQuality.Perfect;
                foreach (var thing3 in list)
                {
                    if (thing3.def.thingClass == typeof(Building_Door))
                    {
                        return PlaceSpotQuality.Bad;
                    }
                    var pawn = thing3 as Pawn;
                    if (pawn != null)
                    {
                        if (pawn.Downed)
                        {
                            return PlaceSpotQuality.Bad;
                        }
                        if (placeSpotQuality > PlaceSpotQuality.Okay)
                        {
                            placeSpotQuality = PlaceSpotQuality.Okay;
                        }
                    }
                    if (thing3.def.category == ThingCategory.Plant && thing3.def.selectable && placeSpotQuality > PlaceSpotQuality.Okay)
                    {
                        placeSpotQuality = PlaceSpotQuality.Okay;
                    }
                }
                return placeSpotQuality;
            }
            if (!center.CanReach(c, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly))
            {
                return PlaceSpotQuality.Awful;
            }
            return PlaceSpotQuality.Okay;
        }
    }
}
