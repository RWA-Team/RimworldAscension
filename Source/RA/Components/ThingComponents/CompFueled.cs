using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using static RA.RA_Motes;

namespace RA
{
    public class CompFueled : ThingComp, IThingContainerOwner
    {
        public const float MinBurningTemp = 300f;
        public const float HeatChangePerTick = 0.5f;

        public float fuelStackRefuelPercent = 0.5f;

        public CompFueled_Properties Properties => (CompFueled_Properties)props;

        public CompFueled compFueled;
        public CompHeatPusher compHeatPusher;
        public CompGlower compGlower;
        public CompFlickable compFlickable;

        public ThingContainer fuelContainer;

        public ITab_Fuel fuelTab = new ITab_Fuel();

        public ThingFilter filterFuelPossible = new ThingFilter();
        public ThingFilter filterFuelCurrent = new ThingFilter();

        public Graphic fireGraphic, fuelGraphic;

        public int currentFuelBurnDuration;

        public float currentFuelMaxTemp, internalTemp, heatPerSecond_fromXML, glowRadius_fromXML;

        // required to use ThingContainer
        public ThingContainer GetContainer()
        {
            return fuelContainer;
        }
        // required to use ThingContainer
        public IntVec3 GetPosition()
        {
            return parent.PositionHeld;
        }

        public override void PostSpawnSetup()
        {
            fuelContainer = new ThingContainer(this, false);

            // internal temperature initialize
            internalTemp = parent.Position.GetTemperature();

            // required comps initialize
            compFueled = parent.TryGetComp<CompFueled>();
            compFlickable = parent.TryGetComp<CompFlickable>();
            compHeatPusher = parent.TryGetComp<CompHeatPusher>();
            heatPerSecond_fromXML = compHeatPusher.Props.heatPerSecond;
            compGlower = parent.TryGetComp<CompGlower>();
            glowRadius_fromXML = compGlower.Props.glowRadius;

            // initialize glower and heater
            AdjustGlowerAndHeater();

            // filters initialize
            filterFuelPossible.SetDisallowAll();
            filterFuelPossible.allowedQualitiesConfigurable = false;
            filterFuelPossible.allowedHitPointsConfigurable = false;
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(def => !def.statBases.NullOrEmpty() && def.statBases.Exists(stat => stat.stat.defName == "MaxBurningTempCelsius" && stat.value > 0)))
            {
                filterFuelPossible.SetAllow(thingDef, true);
            }
            filterFuelCurrent.CopyFrom(filterFuelPossible);
        }

        public override void CompTick()
        {
            AdjustInternalTemp();
            AdjustGlowerAndHeater();

            // if building is operational, throw smoke mote and draw fire
            if (Burning && compFueled.Properties.smokeDrawOffset != Vector3.zero)
            {
                // throw smoke puffs
                ThrowSmoke(parent.DrawPos + compFueled.Properties.smokeDrawOffset, parent.RotatedSize.Magnitude / 4);
            }
        }

        public virtual void ThrowSmoke(Vector3 loc, float size)
        {
            ThrowSmokeWhite(loc, size);
        }

        public bool ManuallyOperated
        {
            get
            {
                var pawn = Find.ThingGrid.ThingAt<Pawn>(parent.InteractionCell);
                if (pawn != null && !pawn.pather.Moving
                    && pawn.CurJob != null && pawn.CurJob.targetA != null && pawn.CurJob.targetA.HasThing
                    && pawn.CurJob.targetA.Thing == parent)
                    return true;
                return false;
            }
        }

        public bool Spawned => parent.Spawned;

        public bool Burning => internalTemp >= MinBurningTemp;

        public bool ShouldAutoConsume => compFlickable?.SwitchIsOn ?? false;

        // no fuel or fuel stack is too small
        public bool RequireMoreFuel => fuelContainer.Count == 0 ||
                                       (fuelContainer[0].stackCount / (float)fuelContainer[0].def.stackLimit <=
                                        fuelStackRefuelPercent);

        // adjust internal temperature according to the current inner temperature
        public void AdjustInternalTemp()
        {
            if (currentFuelBurnDuration > 0)
            {
                currentFuelBurnDuration--;

                // if fuel still burning, increase internal temp up to the fuel cap
                if (internalTemp < currentFuelMaxTemp)
                {
                    // limits the increase value to the fuel stat
                    internalTemp += HeatChangePerTick + internalTemp < currentFuelMaxTemp
                        ? HeatChangePerTick
                        : currentFuelMaxTemp - internalTemp;
                }
            }
            // if nothing burns, lower inner temperature
            else
            {
                // try burn next fuel unit
                if (fuelContainer.FirstOrDefault()?.stackCount > 0 && ShouldAutoConsume ||
                    (internalTemp < compFueled.Properties.operatingTemp && ManuallyOperated))
                {
                    currentFuelBurnDuration =
                        (int)fuelContainer[0].GetStatValue(StatDef.Named("BurnDurationHours")) *
                        GenDate.TicksPerHour;
                    currentFuelMaxTemp = fuelContainer[0].GetStatValue(StatDef.Named("MaxBurningTempCelsius"));
                    if (--fuelContainer.FirstOrDefault().stackCount == 0)
                        fuelContainer.Clear();
                }
                else
                {
                    // lower inner temperature
                    var surroundTemp = parent.Position.GetTemperature();
                    if (internalTemp > surroundTemp)
                    {
                        // limits the decrease value to the local temperature
                        internalTemp -= internalTemp - HeatChangePerTick > surroundTemp
                            ? HeatChangePerTick
                            : internalTemp - surroundTemp;
                    }
                }
            }
        }

        // adjust glow and heating according to the current inner temperature
        public void AdjustGlowerAndHeater()
        {
            if (Burning)
            {
                if (internalTemp <= compFueled.Properties.operatingTemp)
                {
                    compHeatPusher.Props.heatPerSecond = heatPerSecond_fromXML + (compHeatPusher.Props.heatPushMaxTemperature - heatPerSecond_fromXML) * Mathf.Min(internalTemp / compFueled.Properties.operatingTemp, 1f);
                    compGlower.Props.glowRadius = glowRadius_fromXML * Mathf.Min(internalTemp / compFueled.Properties.operatingTemp, 1);
                }
            }
            else
            {
                compHeatPusher.Props.heatPerSecond = 0;
                compGlower.Props.glowRadius = 0;
            }

            // redraw glower
            Find.MapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
            Find.GlowGrid.RegisterGlower(compGlower);
        }

        // adds graphic overlays to the burner
        public override void PostDraw()
        {
            TryDrawCurrentFuel(parent.DrawPos + compFueled.Properties.fuelDrawOffset);
            TryDrawRandomFire(parent.DrawPos + compFueled.Properties.fireDrawOffset);

            if (!Burning && RequireMoreFuel)
            {
                OverlayDrawer.DrawOverlay(parent, OverlayTypes.OutOfFuel);
            }
        }

        public void TryDrawRandomFire(Vector3 drawLoc)
        {
            if (Burning && compFueled.Properties.fireDrawOffset != Vector3.zero)
            {
                var fireScale = Mathf.Min(internalTemp / compFueled.Properties.operatingTemp, compFueled.Properties.fireDrawScale);
                fireGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Fire", ShaderDatabase.TransparentPostLight, new Vector2(fireScale, fireScale), Color.white);

                fireGraphic.Draw(drawLoc, Rot4.North, parent);
            }
        }

        public void TryDrawCurrentFuel(Vector3 drawLoc)
        {
            if (fuelContainer.Count > 0 && compFueled.Properties.fuelDrawOffset != Vector3.zero)
            {
                var fuelScale = compFueled.Properties.fuelDrawScale;
                
                var count = fuelContainer.FirstOrDefault().Graphic as Graphic_StackCount;
                fuelGraphic = count != null
                    ? count.SubGraphicFor(fuelContainer.FirstOrDefault())
                    : fuelContainer.FirstOrDefault().Graphic;
                
                fuelGraphic.drawSize = new Vector2(fuelScale, fuelScale);
                fuelGraphic.Draw(drawLoc, Rot4.North, parent);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Deep.LookDeep(ref filterFuelCurrent, "filterFuelCurrent");
            Scribe_Values.LookValue(ref fuelStackRefuelPercent, "fuelStackRefuelPercent");
            Scribe_Values.LookValue(ref currentFuelBurnDuration, "currentFuelBurnDurationHours");
            Scribe_Values.LookValue(ref internalTemp, "internalTemp");
            Scribe_Values.LookValue(ref heatPerSecond_fromXML, "heatPerSecond_fromXML");
            Scribe_Values.LookValue(ref glowRadius_fromXML, "glowRadius_fromXML");
            Scribe_Deep.LookDeep(ref fuelContainer, "fuelContainer", this);
        }
    }

    public class CompFueled_Properties : CompProperties
    {
        public Vector3 fireDrawOffset = Vector3.zero;
        public Vector3 smokeDrawOffset = Vector3.zero;
        public Vector3 fuelDrawOffset = Vector3.zero;
        public float fuelDrawScale = 1f;
        public float fireDrawScale = 1f;
        public float operatingTemp = 1000f;

        public CompFueled_Properties()
        {
            compClass = typeof(CompFueled);
        }
    }
}