using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class TradeCenter : Building, IThingContainerOwner
    {
        public ThingContainer colonyExchangeContainer, traderExchangeContainer, traderStock;
        public float colonyGoodsCost, traderGoodsCost;

        public List<Thing> pendingItemsCounter = new List<Thing>();
        public Dictionary<ThingDef, int> pendingResourcesCounters = new Dictionary<ThingDef, int>();

        public Pawn trader, negotiator;

        public TradeCenter()
        {
            colonyExchangeContainer = new ThingContainer(this, false);
            traderExchangeContainer = new ThingContainer(this, false);
            traderStock = new ThingContainer(this, false);
        }

        // required for using IThingContainerOwner, to use modified HaulToContainer job (HaulToTrade)
        public IntVec3 GetPosition()
        {
            return Position;
        }

        // required for using IThingContainerOwner, to use modified HaulToContainer job (HaulToTrade)
        public ThingContainer GetContainer()
        {
            return colonyExchangeContainer;
        }

        // area where things are counted as tradeables
        public IEnumerable<IntVec3> TradeableCells
            => FindUtil.SquareAreaAround(Position, Mathf.RoundToInt(def.specialDisplayRadius),
                    this.OccupiedRect().Cells)
                    .Where(cell => cell.Walkable() && cell.InBounds());
        
        // things in tradeable area, except for already pending for sell (only single items, not stackable resources)
        public IEnumerable<Thing> SellablesAround =>
            from cell
            in TradeableCells
                from item in Find.ThingGrid.ThingsAt(cell)
                where
                    item.def.category == ThingCategory.Item && !item.IsBurning() &&
                    !item.IsForbidden(Faction.OfPlayer)
                select item;

        // draw rectangular trade area
        public override void DrawExtraSelectionOverlays()
        {
            if (def.specialDisplayRadius > 1f)
            {
                GenDraw.DrawFieldEdges(TradeableCells.ToList());
            }
            if (def.drawPlaceWorkersWhileSelected && def.PlaceWorkers != null)
            {
                foreach (var worker in def.PlaceWorkers)
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
            if (pendingItemsCounter.Any())
            {
                yield return new FloatMenuOption("Negate current trade deal", NegateTradeDeal);
            }
        }

        public void TryResolveTradeDeal()
        {
            if (trader != null && pendingItemsCounter.NullOrEmpty() && traderExchangeContainer.Any() && TradeBalance >= 0)
            {
                Log.Message("10");
                pendingItemsCounter.Clear();
                pendingResourcesCounters.Clear();

                colonyGoodsCost = 0;
                traderGoodsCost = 0;

                // transfer all sold items and pawns to the trader
                while (colonyExchangeContainer.Any())
                {
                    var transferedThing = colonyExchangeContainer.FirstOrDefault();

                    var transferedPawn = transferedThing as Pawn;
                    if (transferedPawn != null)
                    {
                        transferedPawn.PreTraded(TradeAction.PlayerSells, negotiator, trader);
                        trader.AddToStock(transferedPawn);

                        // give all your pawns and prisoners in the colony negative though about slave trading
                        if (transferedPawn.RaceProps.Humanlike)
                        {
                            foreach (var pawn in Find.MapPawns.FreeColonistsAndPrisoners)
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowPrisonerSold);
                            }
                        }
                    }

                    Log.Message("11");
                    colonyExchangeContainer.TransferToContainer(transferedThing, traderStock, transferedThing.stackCount);
                }

                Log.Message("12");
                // transfer all bought pawns to the colony
                foreach (var thing in traderExchangeContainer)
                {
                    var pawn = thing as Pawn;
                    if (pawn!=null)
                    {
                        pawn.PreTraded(TradeAction.PlayerBuys, negotiator, trader);
                        trader.GiveSoldThingToBuyer(pawn, pawn);
                    }
                }
                Log.Message("13");
                // transfer all bought items around
                traderExchangeContainer.TryDropAll(InteractionCell, ThingPlaceMode.Near);

                negotiator = null;

                Messages.Message("Deal Resolved", MessageSound.Benefit);
            }
        }

        // items returned to their owners
        public void NegateTradeDeal()
        {
            if (traderExchangeContainer.Any() || colonyExchangeContainer.Any())
            {
                // unforbid all possibly forbidden sellables
                foreach (var thing in pendingItemsCounter)
                    thing.SetForbidden(false);
                foreach (var thing in colonyExchangeContainer)
                    thing.SetForbidden(false);

                // scatter offered items around
                colonyExchangeContainer.TryDropAll(InteractionCell, ThingPlaceMode.Near);
                colonyGoodsCost = 0;

                // transfer all unsold items to the trader
                while (traderExchangeContainer.Any())
                {
                    var transferedThing = traderExchangeContainer.FirstOrDefault();
                    traderExchangeContainer.TransferToContainer(transferedThing, traderStock, transferedThing.stackCount);
                }
                traderGoodsCost = 0;

                pendingItemsCounter.Clear();
                pendingResourcesCounters.Clear();

                negotiator = null;

                Messages.Message("Unfinished Deal Negated", MessageSound.Negative);
            }
        }

        public void TraderLeaves()
        {
            NegateTradeDeal();
            
            // transfer all carrier inventory to the trade center
            while (traderStock.Any())
            {
                trader.AddToStock(traderStock.FirstOrDefault());
            }

            trader = null;
        }

        public float TradeBalance => colonyGoodsCost - traderGoodsCost;

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (trader != null)
                NegateTradeDeal();

            base.Destroy(mode);
        }

        public override Graphic Graphic
            => trader == null
                ? def.graphic
                : GraphicDatabase.Get<Graphic_Single>(def.graphic.path + "_Occupied", def.graphic.Shader,
                    def.graphic.drawSize, def.graphic.Color);

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_References.LookReference(ref trader, "trader");
            Scribe_References.LookReference(ref negotiator, "negotiator");

            Scribe_Deep.LookDeep(ref colonyExchangeContainer, "colonyExchangeContainer", this);
            Scribe_Deep.LookDeep(ref traderExchangeContainer, "traderExchangeContainer", this);
            Scribe_Deep.LookDeep(ref traderStock, "traderStock", this);

            Scribe_Collections.LookList(ref pendingItemsCounter, "pendingItemsCounter", LookMode.MapReference);
            Scribe_Collections.LookDictionary(ref pendingResourcesCounters, "pendingResourcesCounters",
                LookMode.DefReference, LookMode.Value);

            Scribe_Values.LookValue(ref colonyGoodsCost, "colonyGoodsCost");
            Scribe_Values.LookValue(ref traderGoodsCost, "traderGoodsCost");
        }
    }
}