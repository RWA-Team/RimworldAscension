using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RA
{
    public static class Extensions
    {
        public static List<Thing> ListByGroup(this ThingRequestGroup group)
        {
            var listsByGroup = typeof(ListerThings).GetField("listsByGroup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Find.ListerThings) as List<Thing>[];
            return listsByGroup[(int)group];
        }
    }
}