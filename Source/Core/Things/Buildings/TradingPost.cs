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
            this.colonyOffer = new ThingContainer(this, false);
        }

        // required for using IThingContainerOwner, to use modified HaulToContainer job (HaulToTrade)
        public IntVec3 GetPosition()
        {
            return this.Position;
        }

        // required for using IThingContainerOwner, to use modified HaulToContainer job (HaulToTrade)
        public ThingContainer GetContainer()
        {
            return this.colonyOffer;
        }

        public IEnumerable<IntVec3> TradeableCells
        {
            get
            {
                // half width of the rectangle, determined by <specialDisplayRadius> in building def
                int tradeZoneRange = (int)this.def.specialDisplayRadius + 1;

                IntVec3 centerCell = this.Position;
                IntVec3 currentCell = centerCell;

                for (int i = centerCell.x - tradeZoneRange; i < centerCell.x + tradeZoneRange + 1; i++)
                {
                    currentCell.x = i;
                    for (int j = centerCell.z - tradeZoneRange; j < centerCell.z + tradeZoneRange + 1; j++)
                    {
                        currentCell.z = j;
                        if ((Math.Abs(centerCell.x - currentCell.x) > 1 || Math.Abs(centerCell.z - currentCell.z) > 1) && GenGrid.InBounds(currentCell) && currentCell.Walkable())
                            yield return currentCell;
                    }
                }
            }
        }

        public IEnumerable<Thing> PotentialSellables
        {
            get
            {
                foreach (IntVec3 cell in this.TradeableCells)
                {
                    foreach (Thing item in Find.ThingGrid.ThingsAt(cell))
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
            if (this.def.specialDisplayRadius > 1f)
            {
                GenDraw.DrawFieldEdges(TradeableCells.ToList());
            }
            if (this.def.drawPlaceWorkersWhileSelected && this.def.PlaceWorkers != null)
            {
                for (int i = 0; i < this.def.PlaceWorkers.Count; i++)
                {
                    this.def.PlaceWorkers[i].DrawGhost(this.def, this.Position, this.Rotation);
                }
            }
            if (this.def.hasInteractionCell)
            {
                GenDraw.DrawInteractionCell(this.def, Position, Rotation);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            ICommunicable communicable = TradeSession.tradeCompany as ICommunicable;

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
                Action action = () =>
                {
                    NegateTradeDeal();
                };
                yield return new FloatMenuOption("Negate Current Deal", action);
            }
            else
            {
                Action action = () =>
                {
                    Job job = new Job(DefDatabase<JobDef>.GetNamed("GoTrading"), new TargetInfo(this))
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
                foreach (Thing thing in colonyOffer)
                {
                    // used to fill all the stacks of the same thing type
                    TradeSession.tradeCompany.AddToStock(thing);
                }
                colonyOffer.Clear();
                // NOTE: merchantOffer Clear?
                foreach (Thing thing in merchantOffer)
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
            else
                return false;
        }

        // items returned to their owners
        public void NegateTradeDeal()
        {
            colonyOffer.TryDropAll(InteractionCell, ThingPlaceMode.Near);
            foreach (Thing thing in merchantOffer)
            {
                TradeSession.tradeCompany.AddToStock(thing);
            }
            merchantOffer.Clear();
            TradeSession.FinishTradeDeal();

            Messages.Message("Unfinished Deal Negated", MessageSound.Negative);
        }

        public void RegenerateRequestCounters()
        {
            List<Thing> currentlyHauled_RequestedItems = new List<Thing>();
            List<ThingCount> currentlyHauled_RequestedResources = new List<ThingCount>();
            Thing currentHaulable;

            // generate lists of things being hauled due to offer requests
            foreach (Pawn pawn in Find.ListerPawns.FreeColonists)
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
            foreach (ThingCount counter in offeredResourceCounters)
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
                else
                {
                    return GraphicDatabase.Get<Graphic_Single>("Things/Buildings/TradingPost/TradingPost_Occupied", def.graphic.Shader, def.graphic.drawSize, def.graphic.Color);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.LookDeep(ref colonyOffer, "colonyOffer", new object[] { this });
            Scribe_Collections.LookList(ref merchantOffer, "merchantOffer", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList(ref offeredItems, "offeredItems", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList(ref offeredResourceCounters, "offeredResourceCounters", LookMode.Deep, new object[] { this });
            Scribe_Deep.LookDeep(ref merchant, "merchant", new object[0]);
            Scribe_Values.LookValue(ref occupied, "occupied");
        }
    }
}