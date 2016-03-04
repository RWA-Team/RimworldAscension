using System.Collections.Generic;
using Verse;

namespace RA
{
    public class CompEquipmentGizmoUser : ThingComp
    {
        public Pawn Owner => parent as Pawn;

        public bool SpawnedAndWell(Pawn pawn)
        {
            if (pawn.SpawnedInWorld && !pawn.Downed)
                return true;
            return false;
        }

        public IEnumerable<CompSlots> EquippedSlottersComps()
        {
            // iterate through all apparels
            foreach (ThingWithComps thing in Owner.apparel.WornApparel)
            // NOTE: check what type to return (If it's designator, command_action, command or gizmo)
            {
                foreach (var thingComp in thing.AllComps.FindAll(comp => comp.GetType() == typeof(CompSlots)))
                {
                    var comp = (CompSlots) thingComp;
                    // reassign pawn owner in comp, if not already set
                    comp.owner = comp.owner != Owner ? Owner : comp.owner;

                    yield return comp;
                }
            }

            // iterate through all equipment (weapons)
            foreach (var thing in Owner.equipment.AllEquipment)
            {
                foreach (var thingComp in thing.AllComps.FindAll(comp => comp.GetType() == typeof(CompSlots)))
                {
                    var comp = (CompSlots) thingComp;
                    // reassign pawn owner in comp, if not already set
                    comp.owner = comp.owner != Owner ? Owner : comp.owner;

                    yield return comp;
                }
            }
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if (SpawnedAndWell(Owner))
            {
                // NOTE: should use concatenation of both enumerables, but can't cast to the same type without error
                // iterate through all apparels
                foreach (ThingWithComps thing in Owner.apparel.WornApparel)
                // NOTE: check what type to return (If it's designator, command_action, command or gizmo)
                {
                    foreach (var thingComp in thing.AllComps.FindAll(comp => comp.GetType().IsSubclassOf(typeof(CompEquipmentGizmoProvider))))
                    {
                        var comp = (CompEquipmentGizmoProvider) thingComp;
                        // reassign pawn owner in comp, if not already set
                        comp.owner = comp.owner != Owner ? Owner : comp.owner;

                        foreach (var gizmo in comp.CompGetGizmosExtra())
                        {
                            yield return gizmo;
                        }
                    }
                }

                // iterate through all equipment (weapons)
                foreach (var thing in Owner.equipment.AllEquipment)
                {
                    foreach (var thingComp in thing.AllComps.FindAll(comp => comp.GetType().IsSubclassOf(typeof(CompEquipmentGizmoProvider))))
                    {
                        var comp = (CompEquipmentGizmoProvider) thingComp;
                        // reassign pawn owner in comp, if not already set
                        comp.owner = comp.owner != Owner ? Owner : comp.owner;

                        foreach (var gizmo in comp.CompGetGizmosExtra())
                        {
                            yield return gizmo;
                        }
                    }
                }
            }
        }
    }
}
