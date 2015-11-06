using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace RA
{
    public class CompEquipmentGizmoProvider : ThingComp
    {
        public Pawn owner;

        public bool ParentIsEquipped
        {
            get
            {
                if (owner != null)
                    if (owner.equipment.AllEquipment.Contains(this.parent) || owner.apparel.WornApparel.Contains(this.parent as Apparel))
                        return true;

                return false;
            }
        }
    }
}
