using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RA
{
    class CompLoot : ThingComp
    {
        public List<ThingCount> LootCounters => (props as CompLoot_Properties).lootCounters;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            Action lootAction = () =>
            {
                DesignateForLooting();
                selPawn.drafter.TakeOrderedJob(new Job(JobDefOf.Deconstruct, parent));
            };
            yield return new FloatMenuOption("Loot the " + parent.Label, lootAction);
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if (Find.DesignationManager.DesignationOn(parent)?.def != DesignationDefOf.Deconstruct)
            {
                // Setup a new command
                var gizmoLoot = new Command_Action
                {
                    // not used, but need to be defined, so that gizmo could accept actions
                    icon = parent.def.uiIcon,
                    //// Command icon
                    //icon = ContentFinder<Texture2D>.Get("Missing"),
                    // Command label
                    defaultLabel = "Loot",
                    // Command description
                    defaultDesc = "Search for useful things inside " + parent.Label,
                    // Command sound when activated
                    activateSound = SoundDef.Named("Click"),
                    // Action to call
                    action = DesignateForLooting
                };
                yield return gizmoLoot;
            }
        }

        public void DesignateForLooting()
        {
            parent.SetFaction(Faction.OfColony);
            Find.DesignationManager.AddDesignation(new Designation(parent, DesignationDefOf.Deconstruct));
        }

        public override void PostDestroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Check destroy mod passed
            if (mode == DestroyMode.Deconstruct)
            {
                SpawnLoot();
            }
        }

        // used to spawn things after scavenge action
        public virtual void SpawnLoot()
        {
            var lootList = new List<Thing>();

            foreach (var counter in LootCounters)
            {
                do
                {
                    var thing = ThingMaker.MakeThing(counter.thingDef);
                    thing.stackCount = Mathf.Min(thing.def.stackLimit, counter.count);
                    counter.count -= thing.stackCount;
                    lootList.Add(thing);
                } while (counter.count > 0);
            }

            foreach (var thing in lootList)
            {
                GenPlace.TryPlaceThing(thing, parent.Position, ThingPlaceMode.Near);
            }
        }
    }

    public class CompLoot_Properties : CompProperties
    {
        public List<ThingCount> lootCounters = new List<ThingCount>();

        // Default requirement
        public CompLoot_Properties()
        {
            compClass = typeof(CompLoot);
        }
    }
}
