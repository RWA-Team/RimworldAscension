using System.Collections.Generic;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;


namespace RA
{
    public class ITab_Fuel : ITab
    {
        public static readonly Texture2D FullTexFuel = SolidColorMaterials.NewSolidColorTexture(new Color(0.7f, 0.7f, 1f));
        public static readonly Texture2D FullTexFuelCount = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0.7f, 1f));
        public static readonly Texture2D FullTexBurnerLow = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.41f, 0f));
        public static readonly Texture2D FullTexBurnerHight = SolidColorMaterials.NewSolidColorTexture(new Color(0.75f, 0f, 0f));
        public static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.gray);
        public static readonly Texture2D IconBGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

        public Building_WorkTable_Fueled burner;

        public ITab_Fuel()
        {
            this.labelKey = "Fuel";
        }

        public override bool IsVisible
        {
            get { return true; }
        }

        protected override void FillTab()
        {
            burner = this.SelThing as Building_WorkTable_Fueled;

            const float MarginSize = 5f;
            const float TextHeight = 25f;

            // height is the total count of used text field heights and margins
            this.size = new Vector2(432f, TextHeight * 6 + MarginSize * 3);

            // make smaller rect with margin size borders
            Rect innerRect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(MarginSize);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.BeginGroup(innerRect);
            {
                // allowed fuel types filter button
                Rect filterRect = new Rect(0f, 0f, innerRect.width / 2, TextHeight);
                {
                    if (Widgets.TextButton(filterRect, "Fuel filter"))
                    {
                        Find.WindowStack.Add(new Window_ThingFilter(burner, 105f));
                    }
                }

                // fuel container info
                Rect fuelRect = new Rect(0f, filterRect.height + MarginSize, filterRect.width, innerRect.height - (filterRect.height + MarginSize));
                // fuel container background
                Widgets.DrawMenuSection(fuelRect, true);
                // contract actual size after drawing BG
                fuelRect = fuelRect.ContractedBy(MarginSize);
                // if fuel tank not mmpty
                if (burner.fuelContainer.Count > 0)
                {

                    if (Widgets.InvisibleButton(fuelRect))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        options.Add(new FloatMenuOption("Fuel Info", () =>
                        {
                            Find.WindowStack.Add(new Dialog_InfoCard(burner.fuelContainer[0]));
                        }));
                        options.Add(new FloatMenuOption("Fuel Drop".Translate(), () =>
                        {
                            burner.fuelContainer.TryDropAll(burner.InteractionCell, ThingPlaceMode.Near);
                        }));

                        Find.WindowStack.Add(new FloatMenu(options, string.Empty));
                    }

                    GUI.BeginGroup(fuelRect);
                    {
                        Thing fuel = burner.fuelContainer[0];

                        // current fuel type icon
                        Rect fuelIconRect = new Rect(0f, 0f, TextHeight * 2, TextHeight * 2);
                        Widgets.DrawTextureFitted(fuelIconRect, IconBGTex, 1);
                        Widgets.ThingIcon(fuelIconRect.ContractedBy(2f), fuel);

                        // burning fillable bar
                        Rect burningLabelRect = new Rect(fuelIconRect.width + MarginSize, 0f, fuelRect.width - (fuelIconRect.width + MarginSize), fuelIconRect.height / 2);
                        Widgets.Label(burningLabelRect, "Burning progress:");
                        Rect burningBarRect = new Rect(burningLabelRect.x, burningLabelRect.height, burningLabelRect.width, burningLabelRect.height);
                        float fillPercentBurningProgress = (float)burner.currentFuelBurnDuration / fuel.GetStatValue(StatDef.Named("BurnDuration"));
                        Widgets.FillableBar(burningBarRect, fillPercentBurningProgress, FullTexFuel, EmptyTex, false);

                        // fuel count fillable bar
                        Rect fuelCountBarRect = new Rect(0f, burningBarRect.yMax + MarginSize, fuelRect.width, 20f);
                        float fillPercentFuelCount = (float)fuel.stackCount / (float)fuel.def.stackLimit;
                        Widgets.FillableBar(fuelCountBarRect, fillPercentFuelCount, FullTexFuelCount, EmptyTex, false);
                        Widgets.Label(fuelCountBarRect, string.Format("Fuel amount: {0}/{1}", fuel.stackCount, fuel.def.stackLimit));

                        Text.Anchor = TextAnchor.MiddleLeft;
                        // current fuel type info
                        Rect fuelEstimatedTimeRect = new Rect(0f, fuelCountBarRect.yMax + MarginSize / 2, fuelRect.width, 20f);
                        Widgets.Label(fuelEstimatedTimeRect, string.Format("Depletes after:\t{0}", TimeInfo(fuel.stackCount * (int)fuel.GetStatValue(StatDef.Named("BurnDuration")))));
                        // current fuel type info
                        Rect fuelMaxTempRect = new Rect(0f, fuelEstimatedTimeRect.yMax, fuelRect.width, fuelEstimatedTimeRect.height);
                        Widgets.Label(fuelMaxTempRect, string.Format("Max tempertarure:\t{0} °C", fuel.GetStatValue(StatDef.Named("MaxBurningTemp"))));
                    }
                    GUI.EndGroup();
                }
                // no fuel
                else
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Medium;
                    Widgets.Label(fuelRect, "No fuel");
                    Text.Font = GameFont.Small;
                }

                // burner info
                Rect burnerRect = new Rect(filterRect.width + MarginSize, 0f, innerRect.width - (fuelRect.width + MarginSize * 3), innerRect.height);
                GUI.BeginGroup(burnerRect);
                {
                    Text.Anchor = TextAnchor.MiddleCenter;

                    // refill percent slider
                    Rect sliderLabelRect = new Rect(0f, 0f, burnerRect.width, TextHeight);
                    Widgets.Label(sliderLabelRect, "Refill at " + burner.fuelStackRefillPercent.ToStringPercent());
                    Rect sliderRect = new Rect(0f, sliderLabelRect.height - 2.5f, burnerRect.width, TextHeight / 2);
                    burner.fuelStackRefillPercent = GUI.HorizontalSlider(sliderRect, burner.fuelStackRefillPercent, 0f, 1f);

                    // burner fillable bar
                    Rect burnerLabelRect = new Rect(0f, sliderRect.yMax, burnerRect.width, TextHeight);
                    Widgets.Label(burnerLabelRect, "Internal temperature:");
                    Rect burnerBarRect = new Rect(0f, burnerLabelRect.yMax, burnerRect.width, TextHeight);
                    float fillPercentRequiredHeat = Mathf.Min((burner.internalTemp / burner.compFueled.Properties.operatingTemp), 1f);
                    Widgets.FillableBar(burnerBarRect, fillPercentRequiredHeat, (fillPercentRequiredHeat == 1 ? FullTexBurnerHight : FullTexBurnerLow), EmptyTex, false);
                    // line, indiciting operating temp, when internal is above that
                    if (fillPercentRequiredHeat == 1)
                        Widgets.DrawLineVertical(burnerBarRect.x + burnerBarRect.width * (1 -burner.compFueled.Properties.operatingTemp / burner.internalTemp), burnerBarRect.y, burnerBarRect.height);
                    Widgets.Label(burnerBarRect, burner.internalTemp.ToString("F1") + " °C");

                    // burner operating temp label
                    Rect burnerOpLabelRect = new Rect(0f, burnerBarRect.yMax, burnerRect.width, TextHeight);
                    Widgets.Label(burnerOpLabelRect, "Operating temperature: " + burner.compFueled.Properties.operatingTemp +" °C");

                    // burner current condition label
                    Rect burnerConditionLabelRect = new Rect(0f, burnerOpLabelRect.yMax, burnerRect.width * 0.55f, burnerRect.height - burnerOpLabelRect.yMax);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(burnerConditionLabelRect, "Current status:");
                    // burner current condition status
                    Rect burnerConditionStatusRect = new Rect(burnerConditionLabelRect.xMax, burnerConditionLabelRect.y, burnerRect.width - burnerConditionLabelRect.width, burnerConditionLabelRect.height);
                    string status;
                    if (burner.internalTemp >= burner.compFueled.Properties.operatingTemp)
                    {
                        status = " working";
                        GUI.color = Color.green;
                    }
                    else
                    {
                        status = " low temp";
                        GUI.color = Color.red;
                    }
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(burnerConditionStatusRect, status);
                    GUI.color = Color.white;
                }
                GUI.EndGroup();
            }
            GUI.EndGroup();
            // resets text anchor to upper left (game default)
            GenUI.ResetLabelAlign();
        }

        public string TimeInfo(int ticks)
        {
            int years, months, days, hours;
            StringBuilder timeInfo = new StringBuilder();
            if ((years = ticks / GenDate.TicksPerYear) > 0)
            {
                timeInfo.Append(years + "y ");
                ticks %= years;
            }
            if ((months = ticks / GenDate.TicksPerMonth) > 0)
            {
                timeInfo.Append(months + "m ");
                ticks %= months;
            }
            if ((days = ticks / GenDate.TicksPerDay) > 0)
            {
                timeInfo.Append(days + "d ");
                ticks %= days;
            }
            if ((hours = ticks / GenDate.TicksPerHour) > 0)
            {
                timeInfo.Append(hours + "h ");
                ticks %= hours;
            }
            if (years == 0 && months == 0 && days == 0 && hours == 0)
            {
                timeInfo.Append("<1h ");
            }
            return timeInfo.ToString();
        }
    }
}