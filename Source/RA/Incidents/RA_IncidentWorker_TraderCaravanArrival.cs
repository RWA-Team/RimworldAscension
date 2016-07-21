using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RA
{
    public class RA_IncidentWorker_TraderCaravanArrival : IncidentWorker_TraderCaravanArrival
    {
        // added tech level requirement
        //protected override bool IsFactionAllowed(Faction faction)
        //{
        //    return base.IsFactionAllowed(faction) && faction.def.techLevel <= FactionDefOf.Colony.techLevel;
        //}

        public override bool TryExecute(IncidentParms parms)
        {
            if (!TryResolveParms(parms))
            {
                return false;
            }
            var pawns = SpawnPawns(parms);
            if (pawns.Count == 0)
            {
                return false;
            }
            foreach (var pawn in pawns)
            {
                if (pawn.needs?.food != null)
                {
                    pawn.needs.food.CurLevel = pawn.needs.food.MaxLevel;
                }
            }
            
            ModifyTradeGroup(ref pawns);

            Find.LetterStack.ReceiveLetter("LetterLabelTraderCaravanArrival".Translate(parms.faction.Name),
                "LetterTraderCaravanArrival".Translate(parms.faction.Name), LetterType.Good, pawns[0]);
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out chillSpot);
            var lordJob = new RA_LordJob_TradeWithColony(parms.faction, chillSpot);
            LordMaker.MakeNewLord(parms.faction, lordJob, pawns);
            return true;
        }

        // disable initial readyness for trade and remove all but one muffalo, transferring all goods there
        public void ModifyTradeGroup(ref List<Pawn> pawns)
        {
            var muffaloChosen = false;
            var firstMuffalo = new Pawn();
            var removeList = new List<Pawn>();
            foreach (var pawn in pawns)
            {
                // disable initial readyness to trade
                pawn.mindState.wantsToTradeWithColony = false;

                // transfer all items from other muffaloes to the main one
                if (pawn.kindDef == PawnKindDef.Named("PackMuffalo"))
                {
                    if (!muffaloChosen)
                    {
                        firstMuffalo = pawn;
                        muffaloChosen = true;
                    }
                    else
                    {
                        for (var i = 0; i < pawn.inventory.container.Count; i++)
                        {
                            firstMuffalo.inventory.container.TryAdd(pawn.inventory.container[i]);
                        }
                        pawn.Destroy();
                        removeList.Add(pawn);
                    }
                }
            }
            pawns = new List<Pawn>(pawns.Except(removeList));
        }
    }
}