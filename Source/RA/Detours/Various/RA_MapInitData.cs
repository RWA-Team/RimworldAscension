using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RA
{
    public static class RA_MapInitData
    {
        // changed starting colonists count to 5
        public static void GenerateDefaultColonistsWithFaction()
        {
            ClearAllColonists();
            if (MapInitData.colonyFaction != null)
            {
                MapInitData.colonyFaction.RemoveAllRelations();
            }
            MapInitData.colonyFaction = FactionGenerator.NewColonyFaction();
            do
            {
                ClearAllColonists();
                for (var i = 0; i < 5; i++)
                {
                    MapInitData.colonists.Add(NewGeneratedColonist());
                }
            }
            while (!MapInitData.AnyoneCanDoRequiredWorks());
        }

        // duplicated cause of inaccessablility
        public static void ClearAllColonists()
        {
            for (var i = MapInitData.colonists.Count - 1; i >= 0; i--)
            {
                MapInitData.colonists[i].relations.ClearAllRelations();
                Find.WorldPawns.PassToWorld(MapInitData.colonists[i], PawnDiscardDecideMode.Discard);
                MapInitData.colonists.RemoveAt(i);
            }
            while (true)
            {
                var pawn = Find.WorldPawns.AllPawnsAliveOrDead.FirstOrDefault(x => x.Faction != null && x.Faction.def == FactionDefOf.Colony);
                if (pawn == null)
                {
                    break;
                }
                Find.WorldPawns.RemovePawn(pawn);
                pawn.relations.ClearAllRelations();
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            }
        }

        // duplicated cause of inaccessablility
        public static Pawn NewGeneratedColonist()
        {
            var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest
            {
                kindDef = PawnKindDefOf.Colonist,
                faction = Faction.OfColony,
                colonistRelationChanceFactor = 26f,
                forceGenerateNewPawn = true
            });
            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            return pawn;
        }
    }
}
