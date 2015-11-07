using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace RA
{
    public class CompSlots : CompEquipmentGizmoProvider, IThingContainerOwner
    {
        public ThingContainer slots;
        public List<Thing> designatedThings = new List<Thing>();

        // gets access to the comp properties
        public CompSlots_Properties Properties
        {
            get
            {
                return (CompSlots_Properties)props;
            }
        }

        // IThingContainerOwner requirement
        public ThingContainer GetContainer()
        {
            return slots;
        }

        // IThingContainerOwner requirement
        public IntVec3 GetPosition()
        {
            return this.parent.Position;
        }

        // initialises ThingContainer owner and restricts the max slots range
        public override void PostSpawnSetup()
        {
            slots = new ThingContainer(this);

            if (Properties.maxSlots > 6)
                Properties.maxSlots = 6;

            if (Properties.maxSlots < 1)
                Properties.maxSlots = 1;
        }

        // apply remaining damage and scatter things in slots, if holder is destroyed
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (this.parent.HitPoints < 0)
            {
                foreach (Thing thing in slots)
                    thing.HitPoints -= (int)totalDamageDealt - this.parent.HitPoints;

                slots.TryDropAll(this.parent.Position, ThingPlaceMode.Near);
            }
        }

        // swap selected equipment and primary equipment
        public void SwapEquipment(ThingWithComps thing)
        {
            // if pawn has equipped weapon
            if (this.owner.equipment.Primary != null)
            {
                ThingWithComps resultThing;
                // put weapon in slotter
                this.owner.equipment.TryTransferEquipmentToContainer(this.owner.equipment.Primary, slots, out resultThing);
            }
            // equip new weapon
            this.owner.equipment.AddEquipment(thing);
            // remove that equipment from slotter
            slots.Remove(thing);

            // interrupt current jobs to prevent random errors
            if (this.owner != null && this.owner.jobs.curJob != null)
                this.owner.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if (ParentIsEquipped)
            {
                yield return new Designator_PutInSlot
                {
                    slotsComp = this,
                    defaultLabel = string.Format("Put in ({0}/{1})", slots.Count, Properties.maxSlots),
                    defaultDesc = string.Format("Put thing in {0}.", this.parent.Label),
                    // not used, but need to be defined, so that gizmo could accept actions
                    icon = this.parent.def.uiIcon
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            // NOTE: check if not "new object[]{ this });"
            Scribe_Deep.LookDeep<ThingContainer>(ref slots, "slots", this);
        }
    }

    public class CompSlots_Properties : CompProperties
    {
        public List<ThingCategoryDef> allowedThingCategoryDefs = new List<ThingCategoryDef>();
        public List<ThingCategoryDef> forbiddenSubThingCategoryDefs = new List<ThingCategoryDef>();
        public int maxSlots = 1;

        // Default requirement
        public CompSlots_Properties()
        {
            this.compClass = typeof(CompSlots_Properties);
        }
    }
}