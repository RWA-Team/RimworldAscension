using System.Collections.Generic;

using RimWorld;
using Verse;
using UnityEngine;

namespace RA
{
    class CompAutoRebuild : ThingComp
    {
        public bool needsAutoRebuild = false;

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            // Toggle fire sustain mode gizmo
            yield return new Command_Toggle
            {
                defaultDesc = "Make your colonists automatically rebuild this " + this.parent.Label + " when it's destroyed.",
                defaultLabel = "Auto rebuild",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade", true),
                isActive = () => needsAutoRebuild,
                toggleAction = () =>
                {
                    // toggle the parameter
                    needsAutoRebuild = !needsAutoRebuild;
                }
            };
        }

        public override void PostDestroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // NOTE: "this" might be unavailable at this point
            GenConstruct.PlaceBlueprintForBuild(this.parent.def.entityDefToBuild, this.parent.Position, this.parent.Rotation, Faction.OfColony, this.parent.Stuff);
        }

        public override void PostExposeData()
        {
            Scribe_Values.LookValue(ref needsAutoRebuild, "needsAutoRebuild");
        }
    }
}
