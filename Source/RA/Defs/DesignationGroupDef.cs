using System.Collections.Generic;
using Verse;

namespace RA
{
    public class DesignationGroupDef : Def
    {
        public List<string> defNames = new List<string>();
        public int order = 0;
        public string designationCategory = string.Empty;
        public string iconPath = string.Empty;
    }
}