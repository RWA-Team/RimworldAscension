using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Designator_ZoneAdd_Growing : Designator_ZoneAdd
    {
        protected override string NewZoneLabel => "GrowingZone".Translate();

        public RA_Designator_ZoneAdd_Growing()
        {
            zoneTypeToPlace = typeof(Zone_Growing);
            defaultLabel = "GrowingZone".Translate();
            defaultDesc = "DesignatorGrowingZoneDesc".Translate();
            LongEventHandler.ExecuteWhenFinished(
                () => icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Growing"));
            hotKey = KeyBindingDefOf.Misc2;
            tutorHighlightTag = "DesignatorZoneCreateGrowing";
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!base.CanDesignateCell(c).Accepted)
            {
                return false;
            }
            return !(Find.FertilityGrid.FertilityAt(c) < ThingDefOf.PlantPotato.plant.fertilityMin);
        }

        protected override Zone MakeNewZone()
        {
            ConceptDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
            return new RA_Zone_Growing();
        }
    }
}
