
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_ConstructDeliverResourcesToBlueprints : WorkGiver_ConstructDeliverResources
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Blueprint);

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            if (t.Faction != pawn.Faction)
            {
                return null;
            }
            var blueprint = t as RimWorld.Blueprint;
            if (blueprint == null)
            {
                return null;
            }
            var thing = blueprint.FirstBlockingThing(pawn);
            if (thing != null)
            {
                if (thing.def.category == ThingCategory.Plant)
                {
                    // SINGLE CHANGE FROM VANILLA WorkGiver, make designation instead of immediate cutting plants
                    //return new Job(JobDefOf.CutPlant, thing);

                    if (Find.DesignationManager.DesignationOn(thing, DesignationDefOf.CutPlant) == null && Find.DesignationManager.DesignationOn(thing, DesignationDefOf.HarvestPlant) == null)
                        Find.DesignationManager.AddDesignation(new Designation(thing, DesignationDefOf.CutPlant));
                    return null;
                }
                if (thing.def.category == ThingCategory.Item)
                {
                    if (thing.def.EverHaulable)
                    {
                        return HaulAIUtility.HaulAsideJobFor(pawn, thing);
                    }
                    Log.ErrorOnce(string.Concat("Never haulable ", thing, " blocking ", t, " at ", t.Position), 6429262);
                }
                return null;
            }
            if (!GenConstruct.CanConstruct(blueprint, pawn))
            {
                return null;
            }
            var job = DeconstructExistingEdificeJob(pawn, blueprint);
            if (job != null)
            {
                return job;
            }
            var job2 = ResourceDeliverJobFor(pawn, blueprint);
            if (job2 != null)
            {
                return job2;
            }
            var job3 = NoCostFrameMakeJobFor(pawn, blueprint);
            return job3;
        }

        public Job DeconstructExistingEdificeJob(Pawn pawn, RimWorld.Blueprint blue)
        {
            if (!blue.def.entityDefToBuild.IsEdifice())
            {
                return null;
            }
            Thing thing = null;
            var cellRect = blue.OccupiedRect();
            for (var i = cellRect.minZ; i <= cellRect.maxZ; i++)
            {
                var j = cellRect.minX;
                while (j <= cellRect.maxX)
                {
                    var c = new IntVec3(j, 0, i);
                    thing = c.GetEdifice();
                    if (thing != null)
                    {
                        var thingDef = blue.def.entityDefToBuild as ThingDef;
                        if (thingDef != null && thingDef.building.canPlaceOverWall && thing.def == ThingDefOf.Wall)
                        {
                            return null;
                        }
                        break;
                    }
                    j++;
                }
                if (thing != null)
                {
                    break;
                }
            }
            if (thing == null || !pawn.CanReserve(thing))
            {
                return null;
            }
            return new Job(JobDefOf.Deconstruct, thing)
            {
                ignoreDesignations = true
            };
        }

        public Job NoCostFrameMakeJobFor(Pawn pawn, IConstructible c)
        {
            if (c is Blueprint_Install)
            {
                return null;
            }
            if (c is RimWorld.Blueprint && c.MaterialsNeeded().Count == 0)
            {
                return new Job(JobDefOf.PlaceNoCostFrame)
                {
                    targetA = (Thing)c
                };
            }
            return null;
        }
    }
}
