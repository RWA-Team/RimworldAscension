using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class Building_TradingPost : Building, IThingContainerOwner
    {
        public ThingContainer colonyOffer;
        public List<Thing> merchantOffer = new List<Thing>();

        public List<Thing> offeredItems = new List<Thing>();
        public List<ThingCount> offeredResourceCounters = new List<ThingCount>();

        public Pawn merchant;

        public bool occupied;

        public Building_TradingPost()
        {
            colonyOffer = new ThingContainer(this, false);
        }

        // required for using IThingContainerOwner, to use modified HaulToContainer job (HaulToTrade)
        public IntVec3 GetPosition()
        {
            return Position;
        }

        // required for using IThingContainerOwner, to use modified HaulToContainer job (HaulToTrade)
        public ThingContainer GetContainer()
        {
            return colonyOffer;
        }

        public IEnumerable<IntVec3> TradeableCells
        {
            get
            {
                // half width of the rectangle, determined by <specialDisplayRadius> in building def
                var tradeZoneRange = (int)def.specialDisplayRadius + 1;

                var centerCell = Position;
                var currentCell = centerCell;

                for (var i = centerCell.x - tradeZoneRange; i < centerCell.x + tradeZoneRange + 1; i++)
                {
                    currentCell.x = i;
                    for (var j = centerCell.z - tradeZoneRange; j < centerCell.z + tradeZoneRange + 1; j++)
                    {
                        currentCell.z = j;
                        if ((Math.Abs(centerCell.x - currentCell.x) > 1 || Math.Abs(centerCell.z - currentCell.z) > 1) && currentCell.InBounds() && currentCell.Walkable())
                            yield return currentCell;
                    }
                }
            }
        }

        public IEnumerable<Thing> PotentialSellables
        {
            get
            {
                foreach (var cell in TradeableCells)
                {
                    foreach (var item in Find.ThingGrid.ThingsAt(cell))
                    {
                        if (item.def.category == ThingCategory.Item && !item.IsBurning() && !item.IsForbidden(Faction.OfColony))
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            if (def.specialDisplayRadius > 1f)
            {
                GenDraw.DrawFieldEdges(TradeableCells.ToList());
            }
            if (def.drawPlaceWorkersWhileSelected && def.PlaceWorkers != null)
            {
                foreach (PlaceWorker worker in def.PlaceWorkers)
                {
                    worker.DrawGhost(def, Position, Rotation);
                }
            }
            if (def.hasInteractionCell)
            {
                GenDraw.DrawInteractionCell(def, Position, Rotation);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            var communicable = TradeSession.tradeCompany as ICommunicable;

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                yield return new FloatMenuOption("Cannot use: incapable of talking", null);
            }
            else if (occupied == false || merchant == null || merchant.Downed || merchant.Dead)
            {
                yield return new FloatMenuOption("Cannot use: no one to trade with", null);
            }
            else if (TradeSession.deal != null && TradeSession.deal.isPending)
            {
                Action action = NegateTradeDeal;
                yield return new FloatMenuOption("Negate Current Deal", action);
            }
            else
            {
                Action action = () =>
                {
                    var job = new Job(DefDatabase<JobDef>.GetNamed("GoTrading"), new TargetInfo(this))
                    {
                        commTarget = communicable
                    };
                    pawn.drafter.TakeOrderedJob(job);
                };
                yield return new FloatMenuOption("Start trading", action);
            }
        }

        public override void Tick()
        {
            if (TradeSession.deal != null && TradeSession.deal.isPending && Find.TickManager.TicksGame % 65 == 0)
            {
                // regenerates things/resource requests to haul to the trading post
                RegenerateRequestCounters();
                TryResolveTradeDeal();
            }
        }

        public bool TryResolveTradeDeal()
        {
            // deal successful, all offered items hauled to the trading post
            if (offeredItems.Count == 0 && offeredResourceCounters.Count == 0)
            {
                foreach (var thing in colonyOffer)
                {
                    // used to fill all the stacks of the same thing type
                    TradeSession.tradeCompany.AddToStock(thing);
                }
                colonyOffer.Clear();
                // NOTE: merchantOffer Clear?
                foreach (var thing in merchantOffer)
                {
                    // used to fill all the stacks of the same thing type and do some other actions and checks
                    colonyOffer.TryAdd(thing);
                }
                // scatter bought items around
                colonyOffer.TryDropAll(InteractionCell, ThingPlaceMode.Near);
                // clears trade deal data
                TradeSession.FinishTradeDeal();

                Messages.Message("Trade Deal Resolved", MessageSound.Benefit);

                return true;
            }
            return false;
        }

        // items returned to their owners
        public void NegateTradeDeal()
        {
            colonyOffer.TryDropAll(InteractionCell, ThingPlaceMode.Near);
            foreach (var thing in merchantOffer)
            {
                TradeSession.tradeCompany.AddToStock(thing);
            }
            merchantOffer.Clear();
            TradeSession.FinishTradeDeal();

            Messages.Message("Unfinished Deal Negated", MessageSound.Negative);
        }

        public void RegenerateRequestCounters()
        {
            var currentlyHauled_RequestedItems = new List<Thing>();
            var currentlyHauled_RequestedResources = new List<ThingCount>();
            Thing currentHaulable;

            // generate lists of things being hauled due to offer requests
            foreach (var pawn in Find.ListerPawns.FreeColonists)
            {
                if (pawn.CurJob != null && pawn.CurJob.def.defName == "HaulToTrade")
                {
                    currentHaulable = pawn.CurJob.targetA.Thing;

                    if (currentHaulable.def.stackLimit > 1)
                    {
                        if (currentlyHauled_RequestedResources.Exists(counter => counter.thingDef == currentHaulable.def))
                            currentlyHauled_RequestedResources.Find(counter => counter.thingDef == currentHaulable.def).count += pawn.CurJob.maxNumToCarry;
                        else
                            currentlyHauled_RequestedResources.Add(new ThingCount(currentHaulable.def, Math.Abs(pawn.CurJob.maxNumToCarry)));
                    }
                    else
                    {
                        currentlyHauled_RequestedItems.Add(currentHaulable);
                    }
                }
            }

            // regenerate request lists in workgiver for the missing items (cause of failed haul jobs)
            WorkGiver_HaulToTrade.requestedItems = new List<Thing>(offeredItems.Except(currentlyHauled_RequestedItems));

            // regenerate request lists in workgiver for the missing resources (cause of failed haul jobs)
            foreach (var counter in offeredResourceCounters)
            {
                // if workgiver has this resource request
                if (WorkGiver_HaulToTrade.requestedResourceCounters.Exists(counter2 => counter2.thingDef == counter.thingDef))
                {
                    // if this resource is being hauled now
                    if (currentlyHauled_RequestedResources.Exists(counter3 => counter3.thingDef == counter.thingDef))
                    {
                        // regenerate the difference between offer count and (request count + hauled count)
                        WorkGiver_HaulToTrade.requestedResourceCounters.Find(counter2 => counter2.thingDef == counter.thingDef).count = counter.count - currentlyHauled_RequestedResources.Find(counter3 => counter3.thingDef == counter.thingDef).count;
                    }
                    else
                    {
                        // regenerate the difference between offer count and request count
                        WorkGiver_HaulToTrade.requestedResourceCounters.Find(counter2 => counter2.thingDef == counter.thingDef).count = counter.count;
                    }
                }
                else
                {
                    // if this offered resource type is being hauled now
                    if (currentlyHauled_RequestedResources.Exists(counter3 => counter3.thingDef == counter.thingDef))
                    {
                        // if it's hauled count less than required count
                        if (counter.count > currentlyHauled_RequestedResources.Find(counter3 => counter3.thingDef == counter.thingDef).count)
                        {
                            // regenerate the difference between offer count and hauled count
                            WorkGiver_HaulToTrade.requestedResourceCounters.Add(new ThingCount(counter.thingDef, counter.count - currentlyHauled_RequestedResources.Find(counter3 => counter3.thingDef == counter.thingDef).count));
                        }
                    }
                    else
                    {
                        // regenerate the difference between offer count and request count
                        WorkGiver_HaulToTrade.requestedResourceCounters.Add(new ThingCount(counter.thingDef, counter.count));
                    }
                }
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (occupied == false)
                {
                    return def.graphic;
                }
                return GraphicDatabase.Get<Graphic_Single>("Things/Buildings/TradingPost/TradingPost_Occupied", def.graphic.Shader, def.graphic.drawSize, def.graphic.Color);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.LookDeep(ref colonyOffer, "colonyOffer", this);
            Scribe_Collections.LookList(ref merchantOffer, "merchantOffer", LookMode.Deep);
            Scribe_Collections.LookList(ref offeredItems, "offeredItems", LookMode.Deep);
            Scribe_Collections.LookList(ref offeredResourceCounters, "offeredResourceCounters", LookMode.Deep, this);
            Scribe_Deep.LookDeep(ref merchant, "merchant");
            Scribe_Values.LookValue(ref occupied, "occupied");
        }
    }
}