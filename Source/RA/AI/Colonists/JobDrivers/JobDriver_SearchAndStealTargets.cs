using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_SearchAndStealTargets : JobDriver
    {
        public TargetIndex targetToSteal = TargetIndex.A;
        public TargetIndex exitLocation = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var searchForTargetToSteal = SearchForTargetToStealInColony();
            yield return searchForTargetToSteal;
            // infinite loop. Can be surpassed inside SearchForTargetToStealInColony toil via special condition
            yield return Toils_Jump.JumpIf(searchForTargetToSteal, () => CurJob.GetTarget(targetToSteal) == null);

            yield return Toils_Reserve.Reserve(targetToSteal);

            yield return Toils_Goto.GotoThing(targetToSteal, PathEndMode.ClosestTouch)
                .JumpIf(() =>
                {
                    var target = CurJob.GetTarget(targetToSteal).Thing;
                    var victim = target as Pawn;
                    if (victim != null)
                    {
                        return !victim.Downed && victim.Awake();
                    }
                    return target.IsBurning() || target.Destroyed;
                },
                    searchForTargetToSteal);


            yield return Toils_Haul.StartCarryThing(targetToSteal);

            IntVec3 exitCell;
            ExitUtility.TryFindClosestExitSpot(pawn, out exitCell);
            CurJob.SetTarget(exitLocation, exitCell);

            yield return Toils_Goto.GotoCell(exitLocation, PathEndMode.OnCell);

            yield return new Toil
            {
                initAction = () =>
                {
                    if (pawn.Position.OnEdge())
                    {
                        pawn.ExitMap();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        // can bash doors
        public Toil SearchForTargetToStealInColony()
        {
            var checkInterval = GenTicks.TickRareInterval;
            var searchRadius = 10f;
            var minimumStealValue = 100f;

            var toil = new Toil();
            toil.initAction = () =>
            {
                var actor = toil.actor;
                CurJob.locomotionUrgency = LocomotionUrgency.Jog;
                actor.pather.StartPath(FindRandomSearchPointInsideColony(), PathEndMode.OnCell);
            };
            toil.tickAction = () =>
            {
                var actor = toil.actor;

                if (actor.IsHashIntervalTick(checkInterval))
                {
                    var traverseParams = TraverseParms.For(actor, Danger.Deadly, TraverseMode.ByPawn, true);

                    // search for kidnap victim
                    Predicate<Thing> validatorSteal = thing =>
                    {
                        var victim = thing as Pawn;
                        return victim.RaceProps.Humanlike && victim.Downed && victim.Faction == Faction.OfColony &&
                               actor.CanReserve(victim);
                    };
                    var targetThing = GenClosest.ClosestThingReachable(actor.Position,
                        ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch,
                        traverseParams, searchRadius, validatorSteal);
                    if (targetThing != null)
                    {
                        actor.CurJob.SetTarget(targetToSteal, targetThing);
                        CurJob.locomotionUrgency = LocomotionUrgency.Sprint;
                        ReadyForNextToil();
                    }
                    else
                    {
                        // search for thing to steal
                        validatorSteal = thing => thing.def.stackLimit == 1
                                                                   && thing.MarketValue >= minimumStealValue
                                                                   && actor.CanReserve(thing);
                        targetThing = GenClosest.ClosestThingReachable(actor.Position,
                            ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.ClosestTouch,
                            traverseParams, searchRadius, validatorSteal);
                        if (targetThing != null)
                        {
                            actor.CurJob.SetTarget(targetToSteal, targetThing);
                            CurJob.locomotionUrgency = LocomotionUrgency.Sprint;
                            ReadyForNextToil();
                        }
                    }
                }
            };

            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        // can't bash doors in that mode
        public static TargetInfo FindRandomSearchPointInsideColony()
        {
            // find colony buildings first or else, find colony pawns
            var target = Find.ListerBuildings.allBuildingsColonistCombatTargets.RandomElement() ??
                           (Thing) Find.MapPawns.FreeColonists.RandomElement();
            // no proper destinationCell
            if (target == null)
            {
                Log.Error("Tried to search for destinationCell cell n colony when no colony pawns or buildings");
                return IntVec3.Invalid;
            }

            // TODO: check case for unreachable cells
            return CellFinder.RandomClosewalkCellNear(target.Position,
                (int) (target.RotatedSize.Magnitude + 1));
        }
    }
}