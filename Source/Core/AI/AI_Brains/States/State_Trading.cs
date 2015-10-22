using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using RimWorld.SquadAI;

namespace RA
{
    // Defend the position near the trading post
    public class State_Trading : State
    {
        public Building_TradingPost tradingPost;
        public Pawn merchant, animal;

        public State_Trading()
        {
        }

        public State_Trading(Building_TradingPost tradingPost)
        {
            this.tradingPost = tradingPost;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<Building_TradingPost>(ref tradingPost, "tradingPost");
        }

        public override void Init()
        {
            merchant = brain.ownedPawns[0];
            animal = brain.ownedPawns[1];

            merchant.Rotation = Rot4.South;

            TradeSession.tradeCompany.things = new List<Thing>(animal.TryGetComp<CompCaravan>().cargo);
            animal.TryGetComp<CompCaravan>().cargo.Clear();

            tradingPost.occupied = true;
            tradingPost.merchant = merchant;

            // force redraw of tradingPost texture
            Find.MapDrawer.MapMeshDirty(tradingPost.Position, MapMeshFlag.Things, false, false);

            Messages.Message("trade initiated", MessageSound.Standard);
        }

        public override void Cleanup()
        {
            if (!tradingPost.TryResolveTradeDeal())
                tradingPost.NegateTradeDeal();
        }

        public override void UpdateAllDuties()
        {
            merchant.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Station"));
            animal.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("EscortTradeCart"), merchant, 6);

            // Duties for guards
            for (int i = 2; i < this.brain.ownedPawns.Count; i++)
            {
                brain.ownedPawns[i].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("EscortTradeCart"), merchant, 6);
            }            
        }
    }
}
