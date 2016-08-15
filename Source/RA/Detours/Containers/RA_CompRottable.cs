using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_CompRottable : CompRottable
    {
        // modifies rotting speed based on container stats
        public override void CompTickRare()
        {
            var initialRotProgress = RotProgress;

            var container = Find.ThingGrid.ThingsListAt(parent.PositionHeld).Find(building => building is Container && building.Spawned) as Container;
            var containerRotFactor = container?.comp.Properties.rotModifier ?? 1f;

            var temperatureForCell = GenTemperature.GetTemperatureForCell(parent.PositionHeld);
            var temperatureRotModifier = GenTemperature.RotRateAtTemperature(temperatureForCell);
            RotProgress += Mathf.RoundToInt(temperatureRotModifier * GenTicks.TickRareInterval) * containerRotFactor;
            if (Stage == RotStage.Rotting && PropsRot.rotDestroys)
            {
                parent.Destroy();
                return;
            }
            // if rotting progressed
            if (Mathf.FloorToInt(initialRotProgress / GenDate.TicksPerDay) != Mathf.FloorToInt(RotProgress / GenDate.TicksPerDay))
            {
                if (Stage == RotStage.Rotting && PropsRot.rotDamagePerDay > 0f)
                {
                    parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(PropsRot.rotDamagePerDay * containerRotFactor), null, null));
                }
                else if (Stage == RotStage.Dessicated && PropsRot.dessicatedDamagePerDay > 0f && ShouldTakeDessicateDamage())
                {
                    parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(PropsRot.dessicatedDamagePerDay * containerRotFactor), null, null));
                }
            }
        }

        private CompProperties_Rottable PropsRot => (CompProperties_Rottable)props;

        private bool ShouldTakeDessicateDamage()
        {
            var thing = parent.holder?.owner as Thing;
            return thing == null || thing.def.category != ThingCategory.Building || !thing.def.building.preventDeterioration;
        }
    }
}
