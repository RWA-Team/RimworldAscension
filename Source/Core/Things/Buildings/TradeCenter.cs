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

        public List<Thing> pendingItemsCounter;
        public Dictionary<ThingDef, int> pendingResourcesCounters;
        
        public Pawn trader, negotiator;

        public TradeCenter()
        {
            colonyExchangeContainer = new ThingContainer(this, false);
            traderExchangeContainer = new ThingContainer(this, false);
            traderStock = new ThingContainer(this, false);

            pendingItemsCounter = new List<Thing>();
            pendingResourcesCounters = new Dictionary<ThingDef, int>();
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
            => FindUtil.SquareAreaAround(Position, (int) def.specialDisplayRadius + Mathf.Max(def.Size.x, def.Size.z)/2)
                .Where(cell => cell.Walkable() && cell.InBounds());
        
        // things in tradeable area, except for already pending for sell (only single items, not stackable resources)
        public IEnumerable<Thing> SellablesAround =>
            from cell
            in TradeableCells
                from item in Find.ThingGrid.ThingsAt(cell)
                where
                    item.def.category == ThingCategory.Item && !item.IsBurning() &&
                    !item.IsForbidden(Faction.OfColony)
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
            if (!NoMoreTradeablesToHaul)
            {
                yield return new FloatMenuOption("Negate current trade deal", NegateTradeDeal);
            }
        }

        public bool NoMoreTradeablesToHaul => pendingItemsCounter.Any();

        // regenerates things/resource requests to haul to the trading post and tries to resolve trade deal every 250 ticks
        public override void TickRare()
        {
            if (trader != null)
            {
                // deal successful
                if (traderExchangeContainer.Any() && TradeBalance >= 0)
                {
                    ResolveTradeDeal();
                }
            }
        }

        public void ResolveTradeDeal()
        {
            // transfer all sold items to the trader
            while (colonyExchangeContainer.Any())
            {
                var transferedThing = colonyExchangeContainer.FirstOrDefault();
                colonyExchangeContainer.TransferToContainer(transferedThing, traderStock, transferedThing.stackCount);
            }
            colonyGoodsCost = 0;

            // transfer prisoners to the colony
            foreach (var pawn in traderExchangeContainer)
            {
                if (pawn is Pawn)
                    TradeUtility.MakePrisonerOfColony(pawn as Pawn);
            }
            traderGoodsCost = 0;

            // scatter bought items around
            traderExchangeContainer.TryDropAll(InteractionCell, ThingPlaceMode.Near);

            Messages.Message("Trade Deal Resolved", MessageSound.Benefit);
        }

        // items returned to their owners
        public void NegateTradeDeal()
        {
            // unforbid all pending forbidden sellables
            foreach (var thing in pendingItemsCounter)
                thing.SetForbidden(false);

            // scatter offered items around
            colonyExchangeContainer.TryDropAll(InteractionCell, ThingPlaceMode.Near);
            colonyGoodsCost = 0;

            // transfer all unsold items to the trader
            while (traderExchangeContainer.Count > 0)
            {
                var transferedThing = traderExchangeContainer.FirstOrDefault();
                traderExchangeContainer.TransferToContainer(transferedThing, traderStock, transferedThing.stackCount);
            }
            traderGoodsCost = 0;

            pendingItemsCounter.Clear();
            pendingResourcesCounters.Clear();

            Messages.Message("Unfinished Deal Negated", MessageSound.Negative);
        }
        
        public float TradeBalance => colonyGoodsCost - traderGoodsCost;

        public float ThingFinalCost(Thing thing, TradeAction tradeAction)
        {
            var priceTypeFactor = trader.TraderKind.PriceTypeFor(thing.def, tradeAction).PriceMultiplier();
            // additional sell price reduction based on SellPriceFactor stat
            var priceSellFactor = thing.GetStatValue(StatDefOf.SellPriceFactor);
            var priceNegotiatorFactor = negotiator.GetStatValue(StatDefOf.TradePriceImprovement);
            // additional sell price reduction based on difficulty baseSellPriceFactor stat
            var priceDifficultyFactor = Find.Storyteller.difficulty.baseSellPriceFactor;
            // small random (seed is TraderKind based) price diviation for variety of values (x0.9-x1.1 multiplier)
            var priceRandomFactor = TradeUtil.RandomPriceFactorFor(trader.RandomPriceFactorSeed, thing.def);
            
            // same price base for selling/buying
            var price = thing.MarketValue * priceTypeFactor * priceRandomFactor;

            if (tradeAction == TradeAction.PlayerSells)
            {
                price *= priceDifficultyFactor * priceSellFactor * priceNegotiatorFactor;

                price *= TradeUtil.TradePricePostFactorCurve.Evaluate(price);
                price = Mathf.Max(price, 0.01f);

                var buyPrice = ThingFinalCost(thing, TradeAction.PlayerBuys);
                if (price > buyPrice)
                {
                    Log.ErrorOnce("Skill of negotitator trying to put sell price above buy price.", 65387);
                    price = buyPrice;
                }
            }
            else
            {
                price /= priceNegotiatorFactor;
            }

            if (price > 99.5f)
            {
                price = Mathf.Round(price);
            }
            return price;
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