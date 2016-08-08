using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_DesignationCategoryDef : DesignationCategoryDef
    {
        public static FieldInfo infoResolvedDesignators;

        // gets/sets hidden resolvedDesignators
        public List<Designator> ResolvedDesignators
        {
            get { return Initializer.GetHiddenValue(typeof(DesignationCategoryDef), this, "resolvedDesignators", infoResolvedDesignators) as List<Designator>; }
            set { Initializer.SetHiddenValue(value, typeof(DesignationCategoryDef), this, "resolvedDesignators", infoResolvedDesignators); }
        }

        public void ResolveDesignators()
        {
            Log.Message("1");
            ResolvedDesignators.Clear();
            foreach (Type current in specialDesignatorClasses)
            {
                Designator designator = null;
                try
                {
                    designator = (Designator)Activator.CreateInstance(current);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat("DesignationCategoryDef", defName, " could not instantiate special designator from class ", current, ".\n Exception: \n", ex.ToString()));
                }
                if (designator != null)
                {
                    ResolvedDesignators.Add(designator);
                }
                Log.Message("2");
            }
            IEnumerable<BuildableDef> enumerable = from tDef in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>())
                                                   where tDef.designationCategory == defName
                                                   select tDef;
            foreach (BuildableDef current2 in enumerable)
            {
                ResolvedDesignators.Add(new Designator_Build(current2));
            }
        }
    }
}
