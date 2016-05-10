using System.Collections.Generic;
using Verse;

namespace RA
{
    public class DesignationSubCategoryDef : Def
    {
        public bool debug = false;
        public List<string> defNames = new List<string>();
        public string designationCategory = string.Empty;
        public GraphicData graphicData = null;
        public int order = 0;
    }
}
