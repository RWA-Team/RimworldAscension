using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class Tradeable_Pawn : Tradeable
    {
        public override Window NewInfoDialog => new Dialog_InfoCard(AnyPawn);

        public override string Label
        {
            get
            {
                var text = base.Label;
                if (AnyPawn.Name != null && !AnyPawn.Name.Numerical)
                {
                    text = text + ", " + AnyPawn.def.label;
                }
                var text2 = text;
                return string.Concat(text2, " (", AnyPawn.gender.GetLabel(), ", ", AnyPawn.ageTracker.AgeBiologicalYearsFloat.ToString("F0"), ")");
            }
        }

        public override string TipDescription
        {
            get
            {
                var str = AnyPawn.MainDesc(true);
                return str + "\n\n" + AnyPawn.def.description;
            }
        }

        public Pawn AnyPawn => (Pawn)AnyThing;

        public override void ResolveTrade()
        {
            if (ActionToDo == TradeAction.ToLaunch)
            {
                var list = thingsColony.Take(-offerCount).Cast<Pawn>().ToList();
                foreach (var pawn in list)
                {
                    pawn.PreSold();
                    pawn.DeSpawn();
                    if (!pawn.RaceProps.Humanlike)
                    {
                        TradeSession.tradeCompany.AddToStock(pawn);
                    }
                    else
                    {
                        foreach (var current in Find.ListerPawns.ColonistsAndPrisoners)
                        {
                            current.needs.mood.thoughts.TryGainThought(ThoughtDefOf.KnowPrisonerSold);
                        }
                    }
                }
            }
            else if (ActionToDo == TradeAction.ToDrop)
            {
                var list2 = thingsTrader.Take(offerCount).Cast<Pawn>().ToList();
                foreach (var pawn2 in list2)
                {
                    TradeSession.tradeCompany.RemoveFromStock(pawn2);
                    DropThing(pawn2);
                    pawn2.SetFaction(Faction.OfColony);
                }
            }
        }
    }
}
