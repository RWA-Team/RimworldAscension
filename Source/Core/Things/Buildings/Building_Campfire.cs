using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace RA
{
    class Building_Campfire : Building_WorkTable_Fueled
    {
        public bool singlalling = false;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            yield return new Command_Action
            {
                defaultLabel = "Send help signal",
                defaultDesc = "Sends a signal to all nearby friendly factions that you need military assistance. Some of them might even send reinforments to you",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade", true),
                activateSound = SoundDef.Named("Click"),
                action = () =>
                {
                    singlalling = true;
                    // sends string signal to all comps which recognized by proper one
                    SendSignal();
                }
            };

            // Toggle burn everything mode gizmo
            yield return new Command_Toggle
            {
                defaultDesc = "Keep burning fuel even when not used.",
                defaultLabel = "Sustain heat",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade", true),
                isActive = () => autoConsumeMode,
                toggleAction = () =>
                {
                    autoConsumeMode = !autoConsumeMode;
                }
            };
        }

        public override void ThrowSmoke(Vector3 loc, float size)
        {
            SpecialMotes.ThrowSmokeBlack_Signal(loc, size);
        }

        public void SendSignal()
        {
            IncidentParms helpTeamParams = IncidentParmsUtility.GenerateThreatPointsParams();
            helpTeamParams.forced = true;
            helpTeamParams.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;

            QueuedIncident queuedIncident = new QueuedIncident(IncidentDef.Named("RaidFriendly"), helpTeamParams);
            queuedIncident.occurTick = Find.TickManager.TicksGame + IncidentWorker_RaidFriendly.HelpDelay.RandomInRange;
            Find.Storyteller.incidentQueue.Add(queuedIncident);
        }
    }
}
