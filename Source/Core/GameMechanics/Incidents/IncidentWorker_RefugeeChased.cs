using System.Linq;
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
            if (!CellFinder.TryFindRandomEdgeCellWith(cell => cell.CanReachColony(), out spawnSpot))
            {
                return false;
            }
            var faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
            var refugee = PawnGenerator.GeneratePawn(PawnKindDef.Named("SpaceRefugee"), faction);
            Faction enemyFac;
            if (!(from f in Find.FactionManager.AllFactions
                  where !f.def.hidden && f.HostileTo(Faction.OfColony)
                  select f).TryRandomElement(out enemyFac))
            {
                return false;
            }
            var text = "RefugeeChasedInitial".Translate(refugee.Name.ToStringFull, refugee.story.adulthood.title.ToLower(), enemyFac.def.pawnsPlural, enemyFac.name, refugee.ageTracker.AgeBiologicalYears);
            text = text.AdjustedFor(refugee);
            var diaNode = new DiaNode(text);
            var diaOption = new DiaOption("RefugeeChasedInitial_Accept".Translate())
            {
                action = delegate
                {
                    GenSpawn.Spawn(refugee, spawnSpot);
                    refugee.SetFaction(Faction.OfColony);
                    Find.CameraMap.JumpTo(spawnSpot);
                    var incidentParms = IncidentParmsUtility.GenerateThreatPointsParams();
                    incidentParms.forced = true;
                    incidentParms.faction = enemyFac;
                    incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                    incidentParms.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;
                    incidentParms.spawnCenter = spawnSpot;
                    incidentParms.points *= 1.35f;
                    var queuedIncident = new QueuedIncident(IncidentDef.Named("RaidEnemy"), incidentParms)
                    {
                        occurTick = Find.TickManager.TicksGame + RaidDelay.RandomInRange
                    };
                    Find.Storyteller.incidentQueue.Add(queuedIncident);
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOption);
            var text2 = "RefugeeChasedRejected".Translate(refugee.NameStringShort);
            var diaNode2 = new DiaNode(text2);
            var diaOption2 = new DiaOption("OK".Translate()) {resolveTree = true};
            diaNode2.options.Add(diaOption2);
            var diaOption3 = new DiaOption("RefugeeChasedInitial_Reject".Translate()) {link = diaNode2};
            diaNode.options.Add(diaOption3);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode));
            return true;
        }
    }
}
