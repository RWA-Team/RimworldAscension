using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using static System.String;
using static RA.RA_Motes;

namespace RA
{
    public class WorkTableFueled : Building_WorkTable, IThingContainerOwner
    {
        public const float MinBurningTemp = 300f;
        public const float HeatChangePerTick = 0.5f;

        public static readonly string fireTex_path = "Overlays/Fire";
        //Fire.BurningSoundDef = SoundDef.Named("FireBurning");

        // defining required comps here for convinience
        public CompFueled compFueled;
        public CompHeatPusher compHeatPusher;
        public CompGlower compGlower;
        public CompFlickable compFlickable;

        public ThingContainer fuelContainer;

        public ITab_Fuel fuelTab = new ITab_Fuel();

        public ThingFilter filterFuelPossible = new ThingFilter();
        public ThingFilter filterFuelCurrent = new ThingFilter();

        public List<Graphic> fireGraphicsVariants = new List<Graphic>();
        public Graphic fireGraphic_current;
        public Graphic fuelGraphic;
        
        public float fuelStackRefillPercent = 0.5f;
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
            return PositionHeld;
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            fuelContainer = new ThingContainer(this, false);

            // internal temperature initialize
            internalTemp = Position.GetTemperature();

            // required comps initialize
            compFueled = this.TryGetComp<CompFueled>();
            compFlickable = this.TryGetComp<CompFlickable>();
            compHeatPusher = this.TryGetComp<CompHeatPusher>();
            heatPerSecond_fromXML = compHeatPusher.Props.heatPerSecond;
            compGlower = this.TryGetComp<CompGlower>();
            glowRadius_fromXML = compGlower.Props.glowRadius;
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

            // fire textures initialize
            var list = ContentFinder<Texture2D>.GetAllInFolder(fireTex_path).ToList();
            foreach (var texture in list)
            {
                fireGraphicsVariants.Add(GraphicDatabase.Get<Graphic_Single>(fireTex_path + "/" + texture.name, ShaderDatabase.TransparentPostLight, def.Size.ToVector2(), Color.white));
            }
            fireGraphic_current = fireGraphicsVariants.RandomElement();
        }

        public override void Tick()
        {
            base.Tick();
            
            AdjustInternalTemp();            
            AdjustGlowerAndHeater();

            // if building is operational, throw smoke mote and draw fire
            if (Burning && compFueled.Properties.smokeDrawOffset != Vector3.zero)
            {
                // throw smoke puffs
                ThrowSmoke(DrawPos + compFueled.Properties.smokeDrawOffset, RotatedSize.Magnitude / 4);
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
                var pawn = Find.ThingGrid.ThingAt<Pawn>(InteractionCell);
                if (pawn != null && !pawn.pather.Moving
                    && pawn.CurJob != null && pawn.CurJob.targetA != null && pawn.CurJob.targetA.HasThing
                    && pawn.CurJob.targetA.Thing == this)
                    return true;
                return false;
            }
        }

        public bool Burning => internalTemp >= MinBurningTemp;

        public bool ShouldAutoConsume => compFlickable?.SwitchIsOn ?? false;

        // used to determine of using bills is possible
        public override bool UsableNow => compFueled == null
                                          || internalTemp > compFueled.Properties.operatingTemp
                                          || fuelContainer.Count > 0 && fuelContainer[0].stackCount > 0;

        // no fuel or fuel stack is too small
        public bool RequireMoreFuel => fuelContainer.Count == 0 ||
                                       (fuelContainer[0].stackCount / (float)fuelContainer[0].def.stackLimit <=
                                        fuelStackRefillPercent);

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
                        (int) fuelContainer[0].GetStatValue(StatDef.Named("BurnDurationHours"))*
                        GenDate.TicksPerHour;
                    currentFuelMaxTemp = fuelContainer[0].GetStatValue(StatDef.Named("MaxBurningTempCelsius"));
                    if (--fuelContainer.FirstOrDefault().stackCount == 0)
                        fuelContainer.Clear();
                }
                else
                {
                    // lower inner temperature
                    var surroundTemp = Position.GetTemperature();
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
            Find.MapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
            Find.GlowGrid.RegisterGlower(compGlower);
        }

        // adds graphic overlays to the burner
        public override void DrawAt(Vector3 drawLoc)
        {
            Graphic.Draw(drawLoc, Rot4.North, this);
            
            TryDrawCurrentFuel(drawLoc + compFueled.Properties.fuelDrawOffset);
            TryDrawRandomFire(drawLoc + compFueled.Properties.fireDrawOffset);

            if (!Burning && RequireMoreFuel)
            {
                OverlayDrawer.DrawOverlay(this, OverlayTypes.OutOfFuel);
            }
        }

        public void TryDrawRandomFire(Vector3 drawLoc)
        {
            if (Burning && compFueled.Properties.fireDrawOffset != Vector3.zero)
            {
                var maxFireScale = compFueled.Properties.fireDrawScale;
                
                // changes fire graphic every 15 ticks async
                if (this.IsHashIntervalTick(15))
                {
                    Graphic temp;
                    // search until different graphic texture
                    do
                    {
                        // TODO: check linked cycle
                        // looped linked cycle: [(fireGraphicsVariants.IndexOf(fireGraphic_current) + 1) % fireGraphicsVariants.Count];
                        temp = fireGraphicsVariants.RandomElement();
                    } while (fireGraphicsVariants.IndexOf(temp) == fireGraphicsVariants.IndexOf(fireGraphic_current));
                    fireGraphic_current = temp;
                }

                var fireScale = Mathf.Min(internalTemp / compFueled.Properties.operatingTemp, maxFireScale);
                fireGraphic_current.drawSize = new Vector2(fireScale, fireScale);
                fireGraphic_current.Draw(drawLoc, Rot4.North, this);
            }
        }

        public void TryDrawCurrentFuel(Vector3 drawLoc)
        {
            if (fuelContainer.Count > 0 && compFueled.Properties.fuelDrawOffset != Vector3.zero)
            {
                var fuelScale = compFueled.Properties.fuelDrawScale;

                var count = fuelContainer[0].Graphic as Graphic_StackCount;
                if (count != null)
                {
                    fuelGraphic = count.SubGraphicFor(fuelContainer[0]);
                }

                fuelGraphic.drawSize = new Vector2(fuelScale, fuelScale);
                fuelGraphic.Draw(drawLoc, Rot4.North, this);
            }
        }

        public override string GetInspectString()
        {
            base.GetInspectString();

            return Concat(Format("Current status: {0}\n", UsableNow ? "working" : "low temperature"));
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.LookDeep(ref filterFuelCurrent, "filterFuelCurrent");
            Scribe_Values.LookValue(ref fuelStackRefillPercent, "fuelStackRefillPercent");
            Scribe_Values.LookValue(ref currentFuelBurnDuration, "currentFuelBurnDurationHours");
            Scribe_Values.LookValue(ref internalTemp, "internalTemp");
            Scribe_Values.LookValue(ref heatPerSecond_fromXML, "heatPerSecond_fromXML");
            Scribe_Values.LookValue(ref glowRadius_fromXML, "glowRadius_fromXML");
            Scribe_Deep.LookDeep(ref fuelContainer, "fuelContainer", this);
        }
    }
}