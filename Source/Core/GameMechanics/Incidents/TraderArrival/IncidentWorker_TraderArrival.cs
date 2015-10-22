using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.SquadAI;

namespace RA
{
    public class IncidentWorker_TraderArrival : IncidentWorker
    {
        //  additional requirements for incidents to appear, becides number of days passed, allowed biomes, earliest day, etc.
        protected override bool StorytellerCanUseNowSub()
        {
            // NOTE: check return null error if no posts
            // check if there are free traing posts in colony or outdoor temperature is suitable for caravan trades
            if (Find.ListerBuildings.AllBuildingsColonistOfClass<Building_TradingPost>().Any(post => post.occupied == false) || GenTemperature.OutdoorTemp > -25.0f || GenTemperature.OutdoorTemp < 45.0f)
                return true;
            else
                return false;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Building_TradingPost tradingPost;
            Pawn animal, merchant;

            string str;
            if (!TryResolveParms(parms, true)) // limited for trader 
                return false;

            List<Pawn> pawns = SpawnPawns(parms);
            if (pawns == null || pawns.Count < 2)
                return false;

            merchant = pawns[0];
            animal = pawns[1];

            if (pawns.Count > 2)
            {
                str = "GroupTradersArrive".Translate(new object[] { parms.faction.name });
                str = GenText.AdjustedFor(str, merchant);
            }
            else
            {
                str = "SingleTraderArrives".Translate(new object[] { parms.faction.name, merchant.Name });
                str = GenText.AdjustedFor(str, merchant);
            }

            tradingPost = FindClosestFreeTradingPost(merchant);
            if (tradingPost == null)
                return false;
            
            // fill caravan's container. Also setups current Trade Company type
            animal.TryGetComp<CompCaravan>().cargo = TradeSession.GenerateTradeCompanyGoods();
            
            string label = "LabelLetterTrader".Translate();
            Find.LetterStack.ReceiveLetter(label, str, LetterType.Good, merchant, null);
            StateGraph stateGraph = GraphMaker_Trader.TradeGraph(tradingPost);
            BrainMaker.MakeNewBrain(parms.faction, stateGraph, pawns);
            return true;
        }

        public bool TryResolveParms(IncidentParms parms, bool limitPoints = false)
        {
            if (!parms.spawnCenter.IsValid)
            {
                RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter);
            }

            if (parms.faction == null)
            {
                // select faction of trade caravan if not specified before
                if (!(from faction in Find.FactionManager.AllFactionsVisible
                      where (faction.def != FactionDefOf.Colony && !faction.HostileTo(Faction.OfColony))// && faction.def.techLevel <= FactionDefOf.Colony.techLevel)
                      select faction).TryRandomElement<Faction>(out parms.faction))
                {
                    return false;
                }
            }

            // rescale early game caravan strength point to spawn guards, when amount of points is too low
            if (parms.points <= 25f)
            {
                float value = Rand.Value;
                if (value < 0.4f)
                {
                    parms.points = (float)Rand.Range(100, 140);
                }
                else if (value < 0.8f)
                {
                    parms.points = (float)Rand.Range(140, 200);
                }
                else
                {
                    parms.points = (float)Rand.Range(200, 500);
                }
            }

            // rescale late game caravan strength point to spawn guards, when amount of points is too high
            if (limitPoints && parms.points >= 200f)
                parms.points = (float)Rand.Range(150, 350);

            // determine mode of caravan arrival (drop, walk in, etc.)
            parms.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;

            return true;
        }

        // NOTE: add exception checks
        public Building_TradingPost FindClosestFreeTradingPost(Pawn pawn)
        {
            return (Building_TradingPost)GenClosest.ClosestThing_Global_Reachable(pawn.Position, Find.ListerThings.AllThings.Where(post => post.GetType() == typeof(Building_TradingPost) && (post as Building_TradingPost).occupied == false), PathEndMode.InteractionCell, TraverseParms.For(pawn));
        }

        public List<Pawn> SpawnPawns(IncidentParms parms)
        {
            List<Pawn> pawns = new List<Pawn>();
            // merchant
            pawns.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, parms.faction));
            // animal
            pawns.Add(PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("CaravanMuffalo"), parms.faction));
            // guards
            pawns.AddRange(PawnGroupMakerUtility.GenerateArrivingPawns(parms).ToList<Pawn>());

            IntVec3 spawnCell;
            for (int i = 0; i < pawns.Count; i++)
            {
                // regenerate slow pawns
                do
                {
                    Log.Message("regenerated value " + pawns[i].GetStatValue(StatDefOf.MoveSpeed));
                    pawns[i] = PawnGenerator.GeneratePawn(pawns[i].kindDef, pawns[i].Faction);
                } while (pawns[i].GetStatValue(StatDefOf.MoveSpeed) / pawns[i].def.statBases.Find(pair => pair.stat == StatDefOf.MoveSpeed).value < 0.9f);

                // merchant
                if (i == 0)
                {
                    spawnCell = parms.spawnCenter;
                    GenSpawn.Spawn(pawns[i], spawnCell);
                }
                // animal
                else if (i == 1)
                {
                    spawnCell = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, 1);
                    GenSpawn.Spawn(pawns[i], spawnCell);
                }
                // guards
                else
                {
                    spawnCell = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, 3);
                    GenSpawn.Spawn(pawns[i], spawnCell);
                }
            }

            return pawns;
        }
    }
}