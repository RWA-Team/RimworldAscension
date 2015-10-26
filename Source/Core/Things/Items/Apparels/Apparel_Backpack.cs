using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace RA
{/*
    public class Apparel_Backpack : Apparel, IThingContainerOwner
    {
        public ThingContainer inventory;
        public int maxItem;

        public int Capacity { get { return (int)this.GetStatValue(DefDatabase<StatDef>.GetNamed("BackpackMaxItem")); } }
        
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            maxItem = Capacity;
        }
        public ThingContainer GetContainer()
        {
            return this.inventory;
        }

        public IntVec3 GetPosition()
        {
            return this.PositionHeld;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<int>(ref maxItem, "maxItem");
        }

        public override void Tick()
        {
            base.Tick();

            // if no wearer, drop everything
            if (wearer == null)
            {
                inventory.TryDropAll(this.Position, ThingPlaceMode.Near);
            }
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            Designator_PutInInventory designator = new Designator_PutInInventory
            {
                backpack = this,
                defaultLabel = string.Format("Put in ({0}/{1})", wearer.inventory.container.Count, Capacity),
                defaultDesc = "Put thing in" + wearer.NameStringShort + "backpack."
            };
            yield return designator;

            Gizmo_BackpackEquipment gizmo = new Gizmo_BackpackEquipment();

            gizmo.backpack = this;
            yield return gizmo;
        }
    }*/
}