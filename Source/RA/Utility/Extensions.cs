using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RA
{
    public static class Extensions
    {
        #region STATIC INFOS

        public static FieldInfo listsByGroupInfo = typeof(ListerThings).GetField("listsByGroup", GenGeneric.BindingFlagsAll);

        public static FieldInfo resolvedDesignatorsInfo = typeof(DesignationCategoryDef).GetField("resolvedDesignators", GenGeneric.BindingFlagsAll);

        #endregion

        #region EXTENSIONS

        public static List<Thing> ListByGroup(this ThingRequestGroup group)
        {
            return (listsByGroupInfo.GetValue(Find.ListerThings) as List<Thing>[])[(int)group];
        }

        public static List<Designator> ResolvedDesignators(this DesignationCategoryDef category)
        {
            return resolvedDesignatorsInfo.GetValue(category) as List<Designator>;
        }

        #endregion
    }
}