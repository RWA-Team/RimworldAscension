using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
	public static class TradeUtility
	{
		public static IEnumerable<Thing> AllSellableThings
		{
			get
			{
                List<Thing> items = new List<Thing>();
                IEnumerable<Building_TradingPost> tradingPosts = Find.ListerBuildings.AllBuildingsColonistOfClass<Building_TradingPost>();
                if (tradingPosts.Count() > 0)
                {
                    foreach (Building_TradingPost building in tradingPosts)
                    {
                        items.AddRange(building.PotentialSellables);
                    }
                }
                return items;
			}
		}

        // Stack things of the same type
		public static bool TradeAsOne(Thing a, Thing b)
		{
			if (a.def.tradeNeverStack || b.def.tradeNeverStack)
			{
				return false;
			}
			if (a.def.category == ThingCategory.Pawn)
			{
				if (b.def != a.def)
				{
					return false;
				}
				if (a.def.race.Humanlike)
				{
					return false;
				}
				Pawn pawn = (Pawn)a;
				Pawn pawn2 = (Pawn)b;
				return pawn.kindDef == pawn2.kindDef && pawn.health.summaryHealth.SummaryHealthPercent >= 0.9999f && pawn2.health.summaryHealth.SummaryHealthPercent >= 0.9999f && pawn.gender == pawn2.gender && (pawn.Name == null || pawn.Name.Numerical) && (pawn2.Name == null || pawn2.Name.Numerical) && pawn.ageTracker.CurLifeStageIndex == pawn2.ageTracker.CurLifeStageIndex && Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - pawn2.ageTracker.AgeBiologicalYearsFloat) <= 1f;
			}
			else
			{
				if (a.def.useHitPoints && Mathf.Abs(a.HitPoints - b.HitPoints) >= 10)
				{
					return false;
				}
				QualityCategory qualityCategory;
				QualityCategory qualityCategory2;
				if (a.TryGetQuality(out qualityCategory) && b.TryGetQuality(out qualityCategory2) && qualityCategory != qualityCategory2)
				{
					return false;
				}
				if (a.def.category == ThingCategory.Item)
				{
					return a.CanStackWith(b);
				}
				Log.Error(string.Concat(new object[]
				{
					"Unknown TradeAsOne pair: ",
					a,
					", ",
					b
				}));
				return false;
			}
		}

		public static bool EverTradeable(ThingDef def)
		{
			return def.tradeability != Tradeability.Never && ((def.category == ThingCategory.Item || def.category == ThingCategory.Pawn) && def.GetStatValueAbstract(StatDefOf.MarketValue, null) > 0f);
		}

		public static bool TradeableNow(Thing t)
		{
			CompRottable compRottable = t.TryGetComp<CompRottable>();
			return compRottable == null || compRottable.Stage < RotStage.Rotting;
		}

		public static void SellThingsOfType(ThingDef resDef, int debt, TradeCompany trader)
		{
			while (debt > 0)
			{
				Thing thing = null;
				foreach (Building_OrbitalTradeBeacon current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_OrbitalTradeBeacon>())
				{
					foreach (IntVec3 current2 in current.TradeableCells)
					{
						foreach (Thing current3 in Find.ThingGrid.ThingsAt(current2))
						{
							if (current3.def == resDef)
							{
								thing = current3;
								goto IL_C9;
							}
						}
					}
				}
				IL_C9:
				if (thing == null)
				{
					Log.Error("Could not find any " + resDef + " to transfer to trader.");
					break;
				}
				int num = Math.Min(debt, thing.stackCount);
				Thing thing2 = thing.SplitOff(num);
				if (trader != null)
				{
					trader.AddToStock(thing2);
				}
				debt -= num;
			}
		}
	}
}
