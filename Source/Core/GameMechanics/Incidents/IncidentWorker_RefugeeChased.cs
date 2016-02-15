using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RA
{
    // Copied from vanilla, bacause it uses IncidentMakerUtility class
    public class IncidentWorker_RefugeeChased : IncidentWorker
    {
        public const float RaidPointsFactor = 1.35f;

        public static readonly IntRange RaidDelay = new IntRange(1000, 2500);

        public override bool TryExecute(IncidentParms parms)
        {
            IntVec3 spawnSpot;
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.CanReachColony(), out spawnSpot))
            {
                return false;
            }
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
            Pawn refugee = PawnGenerator.GeneratePawn(PawnKindDef.Named("SpaceRefugee"), faction, false, 0);
            Faction enemyFac;
            if (!(from f in Find.FactionManager.AllFactions
                  where !f.def.hidden && f.HostileTo(Faction.OfColony)
                  select f).TryRandomElement(out enemyFac))
            {
                return false;
            }
            string text = "RefugeeChasedInitial".Translate(new object[]
			{
				refugee.Name.ToStringFull,
				refugee.story.adulthood.title.ToLower(),
				enemyFac.def.pawnsPlural,
				enemyFac.name,
				refugee.ageTracker.AgeBiologicalYears
			});
            text = text.AdjustedFor(refugee);
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("RefugeeChasedInitial_Accept".Translate());
            diaOption.action = delegate
            {
                GenSpawn.Spawn(refugee, spawnSpot);
                refugee.SetFaction(Faction.OfColony, null);
                Find.CameraMap.JumpTo(spawnSpot);
                IncidentParms incidentParms = IncidentParmsUtility.GenerateThreatPointsParams();
                incidentParms.forced = true;
                incidentParms.faction = enemyFac;
                incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                incidentParms.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;
                incidentParms.spawnCenter = spawnSpot;
                incidentParms.points *= 1.35f;
                QueuedIncident queuedIncident = new QueuedIncident(IncidentDef.Named("RaidEnemy"), incidentParms);
                queuedIncident.occurTick = Find.TickManager.TicksGame + IncidentWorker_RefugeeChased.RaidDelay.RandomInRange;
                Find.Storyteller.incidentQueue.Add(queuedIncident);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            string text2 = "RefugeeChasedRejected".Translate(new object[]
			{
				refugee.NameStringShort
			});
            DiaNode diaNode2 = new DiaNode(text2);
            DiaOption diaOption2 = new DiaOption("OK".Translate());
            diaOption2.resolveTree = true;
            diaNode2.options.Add(diaOption2);
            DiaOption diaOption3 = new DiaOption("RefugeeChasedInitial_Reject".Translate());
            diaOption3.link = diaNode2;
            diaNode.options.Add(diaOption3);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode));
            return true;
        }
    }
}
