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
            foreach (var gizmo in base.GetGizmos()) yield return gizmo;

            yield return new Command_Toggle
            {
                defaultLabel = "CultivateLand",
                defaultDesc = "Make your colonists automatically Cultivate this growing zone.",
                icon = ContentFinder<Texture2D>.Get("Missing"),
                isActive = () => needsCultivation,
                toggleAction = () => { needsCultivation = !needsCultivation; }
            };

            if (needsCultivation)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "FeritilizeLand",
                    defaultDesc = "Make your colonists automatically fertilize the cultivated growing zone.",
                    icon = ContentFinder<Texture2D>.Get("Missing"),
                    isActive = () => needsFertilization,
                    toggleAction = () => { needsFertilization = !needsFertilization; }
                };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref needsCultivation, "needsCultivation");
            Scribe_Values.LookValue(ref needsFertilization, "needsFertilization");
        }
    }
}