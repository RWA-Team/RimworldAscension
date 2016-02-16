using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
	public class CompDryable : ThingComp
	{
		public float dryProgress;

		CompProperties_Dryable PropsDry
		{
			get
			{
				return (CompProperties_Dryable)props;
			}
		}
		
		public virtual string productsList
		{
			get
			{
				var products = PropsDry.dryProducts;
				if (products.Count == 0)
				{
					return "NothingLower".Translate();
				}
				var stringBuilder = new StringBuilder();
				for (int i = 0; i < products.Count; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(i == products.Count - 1 ? " and " : ", ");
					}
					stringBuilder.Append(products[i].defName.label);
				}
				return stringBuilder.ToString();
			}
		}
		
		public virtual float CurrentDryRate(bool includeTemp = true)
		{
			float num = 1f;
			if (parent.PositionHeld.GetTerrain().label.Contains("water"))
			{
				num -= 20f;
			}
			if (Find.WeatherManager.RainRate > 0.01f)
			{
				Thing edifice = parent.PositionHeld.GetEdifice();
				if (((edifice != null && edifice.def.holdsRoof) || !Find.RoofGrid.Roofed(parent.PositionHeld)))
				{
					num -= Find.WeatherManager.RainRate * 4f;
				}
			}
			if (includeTemp)
			{
				float temperature = GenTemperature.GetTemperatureForCell(parent.PositionHeld);
				if (num >= 0f && PropsDry.dryTemperature.Includes(temperature))
				{
					num *= (temperature - PropsDry.dryTemperature.min) /
						(PropsDry.dryTemperature.max - PropsDry.dryTemperature.min);
				}
			}
			return num;
		}
		
		public virtual float Progress
		{
			get
			{
				return dryProgress / (float)PropsDry.TicksToDry;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref dryProgress, "dryProg", 0f);
		}

		public override void CompTickRare()
		{
			dryProgress += (float)Mathf.RoundToInt(CurrentDryRate() * 250f);
			if (dryProgress >= 0)
			{
				if (Progress >= 1f)
				{
					int count = PropsDry.dryProducts.Count();
					foreach (DryProduct product in PropsDry.dryProducts)
					{
						product.HandleProduct(parent, count);
					}
					if (!parent.Destroyed)
					{
						parent.Destroy();
					}
				}
			}
			else
			{
				dryProgress = 0;
			}
		}

		public override string GetDescriptionPart()
		{
			
			return "DryingDescription".Translate(new object[]
			{
			    productsList,
				PropsDry.dryTemperature.max.ToStringTemperature(),
				PropsDry.TicksToDry.TicksToDaysExtendedString()
			});
		}
		
		
		public override string CompInspectStringExtra()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("PercentDried".Translate(new object[]
			{
				Progress.ToStringPercent()
			}) + " (" + "XDaysOfOptimalDryingLeft".Translate(new object[]
			{
				((int)(PropsDry.TicksToDry - dryProgress)).TicksToDaysExtendedString()
			}) + ")");
			stringBuilder.Append("DryingRate".Translate() + ": " + CurrentDryRate().ToStringPercent());
			if (CurrentDryRate(false) <= 0f)
			{
				stringBuilder.Append(" (" + "ExposedToWaterOrRain".Translate() + ")");
			}
			else if (CurrentDryRate() <= 0f)
			{
				stringBuilder.Append(" (" + "TooColdToDry".Translate() + ")");
			}
			return stringBuilder.ToString();
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(parent.stackCount + count);
			float to = ((ThingWithComps)otherStack).GetComp<CompDryable>().dryProgress;
			dryProgress = Mathf.Lerp(dryProgress, to, t);
		}

		public override void PostSplitOff(Thing piece)
		{
			((ThingWithComps)piece).GetComp<CompDryable>().dryProgress = dryProgress;
		}
	}
	
	public class CompProperties_Dryable : CompProperties
	{
		public List<DryProduct> dryProducts;
		
		public float daysToDry = 4f;
		
		public FloatRange dryTemperature = new FloatRange(0f, 30f);
		
		public int TicksToDry
		{
			get
			{
				return Mathf.RoundToInt(daysToDry * (float)GenDate.TicksPerDay);
			}
		}

		public CompProperties_Dryable()
		{
			this.compClass = typeof(CompDryable);
		}
	}
	
	public class DryProduct
	{
		public ThingDef defName;
		
		public float conversionFactor = 1f;
		
		public bool carryRotProgress;
		
		public bool carryDryProgress;
		
		public bool carryStuff;
		
		public bool carryDefAsStuff;
		
		public bool carryHealth = true;
		
		public virtual void HandleProduct(ThingWithComps thing, int productCount)
		{
			ThingDef stuff = null;
			if (carryStuff || carryDefAsStuff)
			{
				stuff = (carryDefAsStuff ? thing.def : thing.Stuff);
			}
			var driedThing = ThingMaker.MakeThing(defName, stuff);
			var driedThingWithComps = driedThing as ThingWithComps;
			if (driedThingWithComps != null)
			{
				if (carryRotProgress)
				{
					var oldRotComp = thing.GetComp<CompRottable>();
					var newRotComp = driedThingWithComps.GetComp<CompRottable>();
					if (oldRotComp != null && newRotComp != null)
					{
						newRotComp.rotProgress += oldRotComp.rotProgress;
					}
				}
				if (carryDryProgress)
				{
					var oldDryComp = thing.GetComp<CompDryable>();
					var newDryComp = driedThingWithComps.GetComp<CompDryable>();
					if (oldDryComp != null && newDryComp != null)
					{
						newDryComp.dryProgress += oldDryComp.dryProgress;
					}
				}
				if (carryHealth)
				{
					driedThing.HitPoints = Mathf.RoundToInt((float)((float)thing.HitPoints / (float)thing.MaxHitPoints) * (float)driedThing.MaxHitPoints);
				}
			}
			driedThing.stackCount = Mathf.RoundToInt((float)thing.stackCount * conversionFactor);
			if (driedThing.stackCount > 0)
			{
				bool select = (thing.def.selectable && driedThing.def.selectable && Find.Selector.SelectedObjects.Contains(thing));
				var position = new IntVec3();
				position.x = thing.PositionHeld.x;
				position.z = thing.PositionHeld.z;
				ThingContainer container = null;
				if (thing.holder != null)
				{
					var owner = thing.holder.owner;
					container = owner.GetContainer();
				}
				if (productCount == 1)
				{
					thing.Destroy();
				}
				GenPlace.TryPlaceThing(driedThing, position, ThingPlaceMode.Near);
				if (container != null)
				{
					container.TryAdd(driedThing);
				}
				if (select)
				{
					Find.Selector.Select(driedThing);
				}
			}
			else
			{
				driedThing.Destroy();
			}
		}
	}
}
