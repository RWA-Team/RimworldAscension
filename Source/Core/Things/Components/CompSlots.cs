using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RA
{
    public class CompSlots : CompEquipmentGizmoProvider, IThingContainerOwner
    {
        public ThingContainer slots;
        public List<Thing> designatedThings = new List<Thing>();

        // gets access to the comp properties
        public CompSlots_Properties Properties => (CompSlots_Properties)props;

        // IThingContainerOwner requirement
        public ThingContainer GetContainer()
        {
            return slots;
        }

        // IThingContainerOwner requirement
        public IntVec3 GetPosition()
        {
            return parent.Position;
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
            if (parent.HitPoints < 0)
            {
                foreach (var thing in slots)
                    thing.HitPoints -= (int)totalDamageDealt - parent.HitPoints;

                slots.TryDropAll(parent.Position, ThingPlaceMode.Near);
            }
        }

        // swap selected equipment and primary equipment
        public void SwapEquipment(ThingWithComps thing)
        {
            // if pawn has equipped weapon
            if (owner.equipment.Primary != null)
            {
                ThingWithComps resultThing;
                // put weapon in slotter
                owner.equipment.TryTransferEquipmentToContainer(owner.equipment.Primary, slots, out resultThing);
            }
            // equip new weapon
            owner.equipment.AddEquipment(thing);
            // remove that equipment from slotter
            slots.Remove(thing);

            // interrupt current jobs to prevent random errors
            if (owner?.jobs.curJob != null)
                owner.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if (ParentIsEquipped)
            {
                yield return new Designator_PutInSlot
                {
                    slotsComp = this,
                    defaultLabel = string.Format("Put in ({0}/{1})", slots.Count, Properties.maxSlots),
                    defaultDesc = string.Format("Put thing in {0}.", parent.Label),
                    // not used, but need to be defined, so that gizmo could accept actions
                    icon = parent.def.uiIcon
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            // NOTE: check if not "new object[]{ this });"
            Scribe_Deep.LookDeep(ref slots, "slots", this);
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
            compClass = typeof(CompSlots_Properties);
        }
    }
}