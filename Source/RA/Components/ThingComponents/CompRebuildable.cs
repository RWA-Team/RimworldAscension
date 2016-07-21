using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class CompAutoRebuild : ThingComp
    {
        public bool needsAutoRebuild;

        public CompAutoRebuild_Properties Props => (CompAutoRebuild_Properties) props;

        public override void PostSpawnSetup()
        {
            needsAutoRebuild = Props.defaultAutoRebuild;
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            yield return new Command_Toggle
            {
                defaultDesc = "Make your colonists automatically rebuild this " + parent.Label + " when it's destroyed.",
                defaultLabel = "Auto rebuild",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade"),
                isActive = () => needsAutoRebuild,
                toggleAction = () => needsAutoRebuild = !needsAutoRebuild
            };
        }

        public override void PostDestroy(DestroyMode mode, bool wasSpawned)
        {
            if (mode == DestroyMode.Kill && needsAutoRebuild)
                GenConstruct.PlaceBlueprintForBuild(parent.def.entityDefToBuild, parent.Position, parent.Rotation,
                    Faction.OfPlayer, parent.Stuff);
        }

        public override void PostExposeData()
        {
            Scribe_Values.LookValue(ref needsAutoRebuild, "needsAutoRebuild");
        }
    }

    public class CompAutoRebuild_Properties : CompProperties
    {
        public bool defaultAutoRebuild = true;

        public CompAutoRebuild_Properties()
        {
            compClass = typeof (CompAutoRebuild);
        }
    }
}