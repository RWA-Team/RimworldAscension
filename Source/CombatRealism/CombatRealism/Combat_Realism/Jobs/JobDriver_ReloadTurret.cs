﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Combat_Realism
{
    public class JobDriver_ReloadTurret : JobDriver
    {
        private CompAmmoUser _compReloader;
        private CompAmmoUser compReloader
        {
            get
            {
                if (_compReloader == null)
                {
                    Building_TurretGunCR turret = TargetThingA as Building_TurretGunCR;
                    if (turret != null)
                    {
                        _compReloader = turret.compAmmo;
                    }
                }
                return _compReloader;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);


            // Haul ammo
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.A, null, false);

            // Wait in place
            var waitToil = new Toil();
            waitToil.initAction = new Action(delegate
            {
                waitToil.actor.pather.StopDead();
                compReloader.TryStartReload();
            });
            waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
            waitToil.defaultDuration = Mathf.CeilToInt(compReloader.Props.reloadTicks / pawn.GetStatValue(StatDef.Named("ReloadSpeed")));
            yield return waitToil.WithProgressBarToilDelay(TargetIndex.A);

            //Actual reloader
            var reloadToil = new Toil();
            reloadToil.defaultCompleteMode = ToilCompleteMode.Instant;
            reloadToil.initAction = new Action(delegate
            {
                Building_TurretGunCR turret = TargetThingA as Building_TurretGunCR;
                if (compReloader != null && turret.compAmmo != null)
                {
                    compReloader.LoadAmmo(TargetThingB);
                }
            });
            reloadToil.EndOnDespawnedOrNull(TargetIndex.B);
            yield return reloadToil;
        }
    }
}
