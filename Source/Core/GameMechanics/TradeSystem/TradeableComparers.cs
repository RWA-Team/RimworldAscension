using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RA
{
    public abstract class TradeableComparer : IComparer<Tradeable>
    {
        public abstract int Compare(Tradeable lhs, Tradeable rhs);
    }

    public class TradeableComparer_None : TradeableComparer
    {
        public override int Compare(Tradeable lhs, Tradeable rhs)
        {
            return 0;
        }
    }

    public class TradeableComparer_Name : TradeableComparer
    {
        public override int Compare(Tradeable lhs, Tradeable rhs)
        {
            return lhs.Label.CompareTo(rhs.Label);
        }
    }

    public class TradeableComparer_Category : TradeableComparer
    {
        public override int Compare(Tradeable lhs, Tradeable rhs)
        {
            var thingDef = lhs.ThingDef;
            var thingDef2 = rhs.ThingDef;
            if (thingDef.category != thingDef2.category)
            {
                return thingDef.category.CompareTo(thingDef2.category);
            }
            var listOrderPriority = lhs.ListOrderPriority;
            var listOrderPriority2 = rhs.ListOrderPriority;
            if (listOrderPriority != listOrderPriority2)
            {
                return listOrderPriority.CompareTo(listOrderPriority2);
            }
            var num = 0;
            if (!lhs.AnyThing.def.thingCategories.NullOrEmpty())
            {
                num = lhs.AnyThing.def.thingCategories[0].index;
            }
            var value = 0;
            if (!rhs.AnyThing.def.thingCategories.NullOrEmpty())
            {
                value = rhs.AnyThing.def.thingCategories[0].index;
            }
            return num.CompareTo(value);
        }
    }

    public class TradeableComparer_MarketValue : TradeableComparer
    {
        public override int Compare(Tradeable lhs, Tradeable rhs)
        {
            return lhs.AnyThing.MarketValue.CompareTo(rhs.AnyThing.MarketValue);
        }
    }

    public class TradeableComparer_Quality : TradeableComparer
    {
        public override int Compare(Tradeable lhs, Tradeable rhs)
        {
            return GetValueFor(lhs).CompareTo(GetValueFor(rhs));
        }

        public int GetValueFor(Tradeable t)
        {
            QualityCategory result;
            if (!t.AnyThing.TryGetQuality(out result))
            {
                return -1;
            }
            return (int)result;
        }
    }

    public class TradeableComparer_HitPointsPercentage : TradeableComparer
    {
        public override int Compare(Tradeable lhs, Tradeable rhs)
        {
            return GetValueFor(lhs).CompareTo(GetValueFor(rhs));
        }

        public float GetValueFor(Tradeable t)
        {
            var anyThing = t.AnyThing;
            var pawn = anyThing as Pawn;
            if (pawn != null)
            {
                return pawn.health.summaryHealth.SummaryHealthPercent;
            }
            if (!anyThing.def.useHitPoints)
            {
                return 1f;
            }
            return anyThing.HitPoints / (float)anyThing.MaxHitPoints;
        }
    }
}