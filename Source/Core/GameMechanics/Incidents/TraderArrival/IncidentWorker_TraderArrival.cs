using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;

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
            return false;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            string str;
            if (!TryResolveParms(parms, true)) // limited for trader 
                return false;

            var pawns = SpawnPawns(parms);
            if (pawns == null || pawns.Count < 2)
                return false;

            var merchant = pawns[0];
            var animal = pawns[1];

            if (pawns.Count > 2)
            {
                str = "GroupTradersArrive".Translate(parms.faction.name);
                str = str.AdjustedFor(merchant);
            }
            else
            {
                str = "SingleTraderArrives".Translate(parms.faction.name, merchant.Name);
                str = str.AdjustedFor(merchant);
            }

            var tradingPost = FindClosestFreeTradingPost(merchant);
            if (tradingPost == null)
                return false;
            
            // fill caravan's container. Also setups current Trade Company type
            animal.TryGetComp<CompCaravan>().cargo = TradeSession.GenerateTradeCompanyGoods();
            
            var label = "LabelLetterTrader".Translate();
            Find.LetterStack.ReceiveLetter(label, str, LetterType.Good, merchant);
            var stateGraph = MerchantGroupAI.MainGraph(tradingPost);
            BrainMaker.MakeNewBrain(parms.faction, stateGraph, pawns);
            return true;
        }

        public bool TryResolveParms(IncidentParms parms, bool limitPoints = false)
        {
            if (!parms.spawnCenter.IsValid)
            {
                RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter);
            }

            // select faction of trade caravan
            if (parms.faction == null)
            {
                if (!(from faction in Find.FactionManager.AllFactionsVisible
                      where faction.def != FactionDefOf.Colony && !faction.HostileTo(Faction.OfColony) && faction.def.techLevel <= FactionDefOf.Colony.techLevel
                      select faction).TryRandomElement(out parms.faction))
                {
                    return false;
                }
            }

            // rescale early game caravan strength point to spawn guards, when amount of points is too low
            if (parms.points <= 100f)
            {
                var value = Rand.Value;
                if (value < 0.4f)
                {
                    parms.points = Rand.Range(100, 140);
                }
                else if (value < 0.8f)
                {
                    parms.points = Rand.Range(140, 250);
                }
                else
                {
                    parms.points = Rand.Range(250, 500);
                }
            }

            // rescale late game caravan strength point to spawn guards, when amount of points is too high
            if (limitPoints && parms.points >= 200f)
                parms.points = Rand.Range(150, 350);

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
            var pawns = new List<Pawn>();
            // merchant
            if (parms.faction.def.techLevel == TechLevel.Neolithic)
            pawns.Add(PawnGenerator.GeneratePawn(PawnKindDef.Named("TribalMerchant"), parms.faction));
            // animal
            pawns.Add(PawnGenerator.GeneratePawn(PawnKindDef.Named("CaravanMuffalo"), parms.faction));
            // guards
            pawns.AddRange(PawnGroupMakerUtility.GenerateArrivingPawns(parms).ToList());

            for (var i = 0; i < pawns.Count; i++)
            {
                // regenerate slow pawns
                do
                {
                    pawns[i] = PawnGenerator.GeneratePawn(pawns[i].kindDef, pawns[i].Faction);
                } while (pawns[i].GetStatValue(StatDefOf.MoveSpeed) / pawns[i].def.statBases.Find(pair => pair.stat == StatDefOf.MoveSpeed).value < 0.9f);

                // merchant
                IntVec3 spawnCell;
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