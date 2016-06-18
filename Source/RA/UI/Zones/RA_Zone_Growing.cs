using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Zone_Growing : Zone_Growing
    {
        public bool needsCultivation, needsFertilization;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Toggle
            {
                defaultLabel = "Cultivate",
                defaultDesc = "Make your colonists automatically Cultivate this growing zone.",
                icon = ContentFinder<Texture2D>.Get("Missing"),
                isActive = () => needsCultivation,
                toggleAction = () => { needsCultivation = !needsCultivation; }
            };

            yield return new Command_Toggle
            {
                defaultLabel = "Feritilize",
                defaultDesc = "Make your colonists automatically fertilize the cultivated growing zone.",
                icon = ContentFinder<Texture2D>.Get("Missing"),
                isActive = () => needsFertilization,
                toggleAction = () => { needsFertilization = !needsFertilization; }
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref needsCultivation, "needsCultivation");
            Scribe_Values.LookValue(ref needsFertilization, "needsFertilization");
        }
    }
}