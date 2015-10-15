using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace RimworldAscension
{
    public class CompCaravan_Properties : CompProperties
    {
        //All textures use Graphic_Multi
        public string cartTexturePath;
        public string wheelTexturePath;
        public string harnessTexturePath;
        public CompCaravan_Properties()
        {
            compClass = typeof(CompCaravan_Properties);
        }
    }
}
