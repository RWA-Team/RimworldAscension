using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace RA
{/*
    public class Designator_PutInInventory : Designator
    {
        public Apparel_Backpack backpack;

        public int numOfContents;

        public Designator_PutInInventory()
        {
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.DesignateHaul;
            this.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/IconPutIn");
            this.activateSound = SoundDef.Named("Click");
        }

        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            if (thing.def.category != ThingCategory.Item && Find.Reservations.IsReserved(thing, Faction.OfColony))
            {
                return "Can't put" + thing + "in backpack.";
            }
            // no free space in backpack
            else if (backpack.inventory.Count >= backpack.Capacity)
                return "Backpack is full.";
            else return true;
        }

        public override void DesignateThing(Thing thing)
        {
            Find.DesignationManager.AddDesignation(new Designation(thing, DesignationDefOf.Haul));
        }

        protected override void FinalizeDesignationSucceeded()
        {
            Job jobNew = new Job(DefDatabase<JobDef>.GetNamed("PutInInventory"));
            jobNew.maxNumToCarry = 1;
            jobNew.targetA = backpack;
            jobNew.targetQueueB = new List<TargetInfo>();
            if (!jobNew.targetQueueB.NullOrEmpty())
                //if (backpack.wearer.drafter.CanTakePlayerJob())
                    backpack.wearer.drafter.TakeOrderedJob(jobNew);
                //else
                //    backpack.wearer.drafter.QueueJob(jobNew);
            DesignatorManager.Deselect();
        }
    }*/
}