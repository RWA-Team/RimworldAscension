using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace RimworldAscension
{
    public class CompUpgradeable : ThingComp
    {
        public static Texture2D Ui_Upgrade = ContentFinder<Texture2D>.Get("Upgrade", true);

        public CompUpgradeable_Properties Properties
        {
            get
            {
                return (CompUpgradeable_Properties)props;
            }
        }
                
        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            Command_Action gizmo = new Command_Action
            {
                defaultDesc = "Upgrade this so you can get advanced version and save some resources",
                defaultLabel = "Upgrade",
                icon = Ui_Upgrade,
                activateSound = SoundDef.Named("Click"),
                action = new Action(UpgradeBuilding),
            };

            if (!ResearchPrereqsFulfilled)
                gizmo.Disable(DisabledReasonString());

            if (Properties.upgradeTargetDef == null || DefDatabase<ThingDef>.GetNamed(Properties.upgradeTargetDef.ToString(), true) == null)
            {
                Log.Error("No such def to upgrade to.");
                yield return null;
            }

            yield return gizmo;
        }

        public string DisabledReasonString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Research Required:");
            foreach (ResearchProjectDef research in Properties.researchPrerequisites)
            {
                if (!research.IsFinished)
                {
                    stringBuilder.AppendLine(research.LabelCap);
                }
            }
            return stringBuilder.ToString();
        }

        public void UpgradeBuilding()
        {
            parent.Destroy();

            Log.Message("upgradeTargetDef stuff =" + GenStuff.DefaultStuffFor(Properties.upgradeTargetDef));
            
            // NOTE: GenStuff.DefaultStuffFor require more default stuff types
            PlaceFrameForBuild(Properties.upgradeTargetDef, parent.Position, parent.Rotation, Faction.OfColony, GenStuff.DefaultStuffFor(Properties.upgradeTargetDef));
        }

        public void PlaceFrameForBuild(BuildableDef sourceDef, IntVec3 center, Rot4 rotation, Faction faction, ThingDef stuff)
        {
            Frame frame = (Frame)ThingMaker.MakeThing(sourceDef.frameDef, stuff);
            frame.SetFactionDirect(faction);
            Log.Message("material needed =" + frame.MaterialsNeeded());
            foreach (ThingCount resource in frame.MaterialsNeeded())
            {
                Log.Message("resource def =" + resource.thingDef);
                Log.Message("resource count =" + resource.count);
                Thing resource1 = ThingMaker.MakeThing(resource.thingDef);
                resource1.stackCount = (int)Math.Round(resource.count * Properties.upgradeDiscountMultiplier);
                frame.resourceContainer.TryAdd(resource1);
            }
            frame.workDone = (int)Math.Round(frame.def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToMake, frame.Stuff) * Properties.upgradeDiscountMultiplier);
            GenSpawn.Spawn(frame, center, rotation);
        }
        
        public bool ResearchPrereqsFulfilled
        {
            get
            {
                foreach (ResearchProjectDef research in Properties.researchPrerequisites)
                {
                    if (!research.IsFinished)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
