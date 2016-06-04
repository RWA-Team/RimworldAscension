using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class CompEquipmentGizmoProvider : ThingComp
    {
        public Pawn owner;

        public bool ParentIsEquipped
        {
            get
            {
                if (owner != null
                    && (owner.equipment.AllEquipment.Contains(parent) || owner.apparel.WornApparel.Contains(parent as Apparel))) return true;

                return false;
            }
        }
    }
}
