﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace RA.CR
{
    public class CompProperties_FireModes : CompProperties
    {
        public int aimedBurstShotCount = -1;    //will default to regular burst setting if not specified in def
        public bool aiUseAimMode = false;
        public bool aiUseBurstMode = false;
        public bool noSingleShot = false;
        public bool noSnapshot = false;

        public CompProperties_FireModes()
        {
            this.compClass = typeof(CompFireModes);
        }
    }
}