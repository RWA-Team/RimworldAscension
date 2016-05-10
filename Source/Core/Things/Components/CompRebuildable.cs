using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    class CompAutoRebuild : ThingComp
    {
        public bool needsAutoRebuild;

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            // Toggle fire sustain mode gizmo
            yield return new Command_Toggle
            {
                defaultDesc = "Make your colonists automatically rebuild this " + parent.Label + " when it's destroyed.",
                defaultLabel = "Auto rebuild",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade"),
                isActive = () => needsAutoRebuild,
                toggleAction = () =>
                {
                    // toggle the parameter
                    needsAutoRebuild = !needsAutoRebuild;
                }
            };
        }

        public override void PostDestroy(DestroyMode mode, bool wasSpawned)
        {
            // NOTE: "this" might be unavailable at this point
            GenConstruct.PlaceBlueprintForBuild(parent.def.entityDefToBuild, parent.Position, parent.Rotation, Faction.OfColony, parent.Stuff);
        }

        public override void PostExposeData()
        {
            Scribe_Values.LookValue(ref needsAutoRebuild, "needsAutoRebuild");
        }
    }
}
