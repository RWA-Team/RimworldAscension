using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace RA.CR
{
    public class CompProperties_TurretInit : CompProperties
    {
        public CompProperties_TurretInit()
        {
            this.compClass = typeof(CompTurretInit);
        }
    }
}
