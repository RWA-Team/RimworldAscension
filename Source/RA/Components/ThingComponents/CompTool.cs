using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class CompTool : CompEquipmentGizmoProvider
    {
        public int wearTicks;
        public bool wasAutoEquipped;

        public CompTool_Properties Props => (CompTool_Properties)props;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            wearTicks = Props.workTicksPerHealthPercent;
        }

        public bool Allows(string workType)
        {
            return Props.workTypes.Contains(workType) || workType == "Hunting" && Props.usedForHunting;
        }

        public void ToolUseTick()
        {
            if (wearTicks-- <= 0)
            {
                parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1));
                wearTicks = Props.workTicksPerHealthPercent;
            }
        }

        // toggle allowance to use this type of weapon as hunting tool
        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            yield return new Command_Toggle
            {
                defaultLabel = "Use for hunting",
                defaultDesc = "Make your colonists automatically use " + parent.def.label + " for hunting.",
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Hunt"),
                isActive = () => Props.usedForHunting,
                toggleAction = () => { Props.usedForHunting = !Props.usedForHunting; }
            };
        }

        public override void PostExposeData()
        {
            Scribe_Values.LookValue(ref wearTicks, "wearTicks");
            Scribe_Values.LookValue(ref Props.usedForHunting, "usedForHuntingp");
            Scribe_Values.LookValue(ref wasAutoEquipped, "wasAutoEquipped");
        }
    }

    public class CompTool_Properties : CompProperties
    {
        public List<string> workTypes = new List<string>();
        public int workTicksPerHealthPercent;
        public bool usedForHunting;

        public CompTool_Properties()
        {
            compClass = typeof(CompTool);
        }
    }
}