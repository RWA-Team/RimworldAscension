using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace RA
{
    public static class Extensions
    {
        #region STATIC INFOS

        public static FieldInfo ListerThingsInfo = typeof(ListerThings).GetField("listsByGroup", GenGeneric.BindingFlagsAll);
        public static FieldInfo DesignationCategoryDefInfo = typeof(DesignationCategoryDef).GetField("resolvedDesignators", GenGeneric.BindingFlagsAll);
        public static FieldInfo TradeDealInfo = typeof(TradeDeal).GetField("tradeables", GenGeneric.BindingFlagsAll);

        #endregion

        #region EXTENSIONS

        public static List<Thing> ListByGroup(this ThingRequestGroup group)
        {
            return (ListerThingsInfo.GetValue(Find.ListerThings) as List<Thing>[])[(int)group];
        }

        public static List<Designator> ResolvedDesignators(this DesignationCategoryDef category)
        {
            return DesignationCategoryDefInfo.GetValue(category) as List<Designator>;
        }

        public static List<Tradeable> Tradeables(this TradeDeal deal)
        {
            return TradeDealInfo.GetValue(deal) as List<Tradeable>;
        }

        #endregion
    }
}