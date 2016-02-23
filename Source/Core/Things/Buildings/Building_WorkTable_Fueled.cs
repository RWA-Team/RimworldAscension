using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace RA
{
    public class Building_WorkTable_Fueled : Building_WorkTable, IThingContainerOwner
    {
        public const float MinBurningTemp = 100f;
        public const float HeatChangePerTick = 0.5f;

        public static readonly string fireTex_path = "Overlays/Fire";
        //Fire.BurningSoundDef = SoundDef.Named("FireBurning");

        // defining required comps here for convinience
        public CompFueled compFueled;
        public CompHeatPusher compHeatPusher;
        public CompGlower compGlower;
        
        public ThingContainer fuelContainer;

        public ITab_Fuel fuelTab = new ITab_Fuel();

        public ThingFilter filterFuelPossible = new ThingFilter();
        public ThingFilter filterFuelCurrent = new ThingFilter();

        public List<Graphic> fireGraphicsVariants = new List<Graphic>();
        public Graphic fireGraphic_current;
        public Graphic fuelGraphic;

        public bool autoConsumeMode;
        public float fuelStackRefillPercent = 0.5f;
        public int currentFuelBurnDuration = 0;

        public float currentFuelMaxTemp, internalTemp, heatPerSecond_fromXML, glowRadius_fromXML;

        // required to use ThingContainer
        public ThingContainer GetContainer()
        {
            return fuelContainer;
        }
        // required to use ThingContainer
        public IntVec3 GetPosition()
        {
            return this.PositionHeld;
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            fuelContainer = new ThingContainer(this, false);

            // internal temperature initialize
            internalTemp = this.Position.GetTemperature();

            // required comps initialize
            compFueled = this.TryGetComp<CompFueled>();
            autoConsumeMode = compFueled.Properties.defaultAutoConsume;
            compHeatPusher = this.TryGetComp<CompHeatPusher>();
            heatPerSecond_fromXML = compHeatPusher.props.heatPerSecond;
            compGlower = this.TryGetComp<CompGlower>();
            glowRadius_fromXML = compGlower.props.glowRadius;
            AdjustGlowerAndHeater();            

            // filters initialize
            filterFuelPossible.SetDisallowAll();
            filterFuelPossible.allowedQualitiesConfigurable = false;
            filterFuelPossible.allowedHitPointsConfigurable = false;
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => !def.statBases.NullOrEmpty() && def.statBases.Exists(stat => stat.stat.defName == "MaxBurningTemp" && stat.value > 0)))
            {
                filterFuelPossible.SetAllow(def, true);
            }
            filterFuelCurrent.CopyFrom(filterFuelPossible);

            // fire textures initialize
            List<Texture2D> list = ContentFinder<Texture2D>.GetAllInFolder(fireTex_path).ToList<Texture2D>();
            for (int i = 0; i < list.Count; i++)
            {
                fireGraphicsVariants.Add(GraphicDatabase.Get<Graphic_Single>(fireTex_path + "/" + list[i].name, ShaderDatabase.TransparentPostLight, this.def.Size.ToVector2(), Color.white));
            }
            fireGraphic_current = fireGraphicsVariants.RandomElement();
        }

        public override void Tick()
        {
            base.Tick();
            
            AdjustInternalTemp();            
            AdjustGlowerAndHeater();

            // if building is operational, throw smoke mote and draw fire
            if (Burning)
            {
                // throw smoke puffs
                ThrowSmoke(this.DrawPos + compFueled.Properties.smokeDrawOffset, this.RotatedSize.Magnitude / 4);
            }
        }

        public virtual void ThrowSmoke(Vector3 loc, float size)
        {
            RA_Motes.ThrowSmokeWhite(loc, size);
        }

        public bool ManuallyOperated
        {
            get
            {
                Pawn pawn = Find.ThingGrid.ThingAt<Pawn>(this.InteractionCell);
                if (pawn != null && !pawn.pather.Moving
                    && pawn.CurJob != null && pawn.CurJob.targetA != null && pawn.CurJob.targetA.HasThing
                    && pawn.CurJob.targetA.Thing == this)
                    return true;
                return false;
            }
        }

        public bool Burning
        {
            get
            {
                if (internalTemp >= MinBurningTemp)
                    return true;
                return false;
            }
        }

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
                    internalTemp += (HeatChangePerTick + internalTemp < currentFuelMaxTemp) ? HeatChangePerTick : currentFuelMaxTemp - internalTemp;
                }
            }
            // else, burn next fuel unit
            else
            {
                // only pawn can burn fuel, and when it's needed
                if (fuelContainer.Count > 0 && fuelContainer[0].stackCount > 0)
                {
                    // burn everthing in auto mode and only if necessary in manual mode
                    if (autoConsumeMode || (internalTemp <= compFueled.Properties.operatingTemp && ManuallyOperated))
                    {
                        currentFuelBurnDuration = (int)fuelContainer[0].GetStatValue(StatDef.Named("BurnDuration"));
                        currentFuelMaxTemp = fuelContainer[0].GetStatValue(StatDef.Named("MaxBurningTemp"));
                        if (--fuelContainer[0].stackCount == 0)
                            fuelContainer.Clear();
                    }
                }
                // if no more fuel to burn, lower inner temperature
                else
                {
                    float surroundTemp = this.Position.GetTemperature();

                    if (internalTemp > surroundTemp)
                    {
                        // limits the decrease value to the local temperature
                        internalTemp -= (internalTemp - HeatChangePerTick > surroundTemp) ? HeatChangePerTick : internalTemp - surroundTemp;
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
                    compHeatPusher.props.heatPerSecond = heatPerSecond_fromXML + (compHeatPusher.props.heatPushMaxTemperature - heatPerSecond_fromXML) * (Mathf.Min(internalTemp / compFueled.Properties.operatingTemp, 1f));
                    compGlower.props.glowRadius = glowRadius_fromXML * (Mathf.Min(internalTemp / compFueled.Properties.operatingTemp, 1));
                }
            }
            else
            {
                compHeatPusher.props.heatPerSecond = 0;
                compGlower.props.glowRadius = 0;
            }

            // redraw glower
            Find.MapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things);
            Find.GlowGrid.RegisterGlower(compGlower);
        }

        // used to determine of using bills is possible
        public override bool UsableNow
        {
            get
            {
                return compFueled == null
                    || internalTemp > compFueled.Properties.operatingTemp
                    || fuelContainer.Count > 0 && fuelContainer[0].stackCount > 0;
            }
        }

        public bool RequireMoreFuel()
        {
            // no fuel or fuel stack is too small
            if (fuelContainer.Count == 0 || ((float)fuelContainer[0].stackCount / (float)fuelContainer[0].def.stackLimit) <= fuelStackRefillPercent)
            {
                return true;
            }

            return false;
        }

        // adds graphic texture to the burner
        public override void DrawAt(Vector3 drawLoc)
        {
            this.Graphic.Draw(drawLoc, Rot4.North, this);

            TryDrawCurrentFuel(drawLoc + compFueled.Properties.fuelDrawOffset);
            TryDrawRandomFire(drawLoc + compFueled.Properties.fireDrawOffset);
        }

        public void TryDrawRandomFire(Vector3 drawLoc)
        {
            if (Burning)
            {
                float maxFireScale = compFueled.Properties.fireDrawScale;

                // changes fire graphic every 15 ticks async
                if (this.IsHashIntervalTick(15))
                {
                    Graphic temp;
                    // search until different graphic texture
                    do
                    {
                        // looped linked cycle: [(fireGraphicsVariants.IndexOf(fireGraphic_current) + 1) % fireGraphicsVariants.Count];
                        temp = fireGraphicsVariants.RandomElement();
                    } while (fireGraphicsVariants.IndexOf(temp) == fireGraphicsVariants.IndexOf(fireGraphic_current));
                    fireGraphic_current = temp;
                }

                float fireScale = Mathf.Min((internalTemp / compFueled.Properties.operatingTemp), maxFireScale);
                fireGraphic_current.drawSize = new Vector2(fireScale, fireScale);
                fireGraphic_current.Draw(drawLoc, Rot4.North, this);
            }
        }

        public void TryDrawCurrentFuel(Vector3 drawLoc)
        {
            if (fuelContainer.Count > 0)
            {
                float fuelScale = compFueled.Properties.fuelDrawScale;

                if (fuelContainer[0].Graphic is Graphic_StackCount)
                {
                    fuelGraphic = (fuelContainer[0].Graphic as Graphic_StackCount).SubGraphicFor(fuelContainer[0]);
                }

                fuelGraphic.drawSize = new Vector2(fuelScale, fuelScale);
                fuelGraphic.Draw(drawLoc, Rot4.North, this);
            }
        }

        public override string GetInspectString()
        {
            string inspectString = base.GetInspectString();

            string text = inspectString;
            return string.Concat(new string[]
            {
                string.Format("Current status: {0}\n", UsableNow ? "working" : "low temperature")
            });
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.LookDeep(ref filterFuelCurrent, "filterFuelCurrent", new object[0]);
            Scribe_Values.LookValue(ref autoConsumeMode, "sustainHeatMode");
            Scribe_Values.LookValue(ref fuelStackRefillPercent, "fuelStackRefillPercent");
            Scribe_Values.LookValue(ref currentFuelBurnDuration, "currentFuelBurnDuration");
            Scribe_Values.LookValue(ref internalTemp, "internalTemp");
            Scribe_Values.LookValue(ref heatPerSecond_fromXML, "heatPerSecond_fromXML");
            Scribe_Values.LookValue(ref glowRadius_fromXML, "glowRadius_fromXML");
            Scribe_Deep.LookDeep(ref fuelContainer, "fuelContainer", new object[] { this });
        }
    }
}