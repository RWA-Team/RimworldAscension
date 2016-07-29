using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using static RA.ArmorUtil;

namespace RA
{
    public class RA_DamageWorker_AddInjury : DamageWorker_AddInjury
    {
        public struct LocalInjuryResult
        {
            public bool wounded, headshot, deflected;

            public BodyPartRecord lastHitPart;
            public float totalDamageDealt;

            public static LocalInjuryResult MakeNew()
            {
                return new LocalInjuryResult
                {
                    wounded = false,
                    headshot = false,
                    deflected = false,
                    lastHitPart = null,
                    totalDamageDealt = 0f
                };
            }
        }

        // flat damage reduction armor system and armor penetration
        public void ApplyDamagePartial(DamageInfo dInfo, Pawn pawn, ref LocalInjuryResult result)
        {
            var exactPartFromDamageInfo = GetExactPartFromDamageInfo(dInfo, pawn);
            if (exactPartFromDamageInfo == null)
            {
                return;
            }
            var flag = !dInfo.InstantOldInjury;
            
            var num = dInfo.Amount;
            if (flag)
            {
                var dInfoLocal = new DamageInfo(dInfo);
                num = GetAfterArmorDamage(pawn, dInfoLocal, exactPartFromDamageInfo);
            }
            if (num < 0.001)
            {
                result.deflected = true;
                return;
            }
            var hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dInfo.Def, pawn, exactPartFromDamageInfo);
            var hediff_Injury = (Hediff_Injury) HediffMaker.MakeHediff(hediffDefFromDamage, pawn);
            hediff_Injury.Part = exactPartFromDamageInfo;
            hediff_Injury.source = dInfo.Source;
            hediff_Injury.sourceBodyPartGroup = dInfo.LinkedBodyPartGroup;
            hediff_Injury.sourceHediffDef = dInfo.LinkedHediffDef;
            hediff_Injury.Severity = num;
            if (dInfo.InstantOldInjury)
            {
                var hediffComp_GetsOld = hediff_Injury.TryGetComp<HediffComp_GetsOld>();
                if (hediffComp_GetsOld != null)
                {
                    hediffComp_GetsOld.IsOld = true;
                }
                else
                {
                    Log.Error(string.Concat("Tried to create instant old injury on Hediff without a GetsOld comp: ",
                        hediffDefFromDamage, " on ", pawn));
                }
            }
            result.wounded = true;
            result.lastHitPart = hediff_Injury.Part;
            if (IsHeadshot(dInfo, hediff_Injury))
            {
                result.headshot = true;
            }
            if (dInfo.InstantOldInjury &&
                (hediff_Injury.def.CompPropsFor(typeof (HediffComp_GetsOld)) == null ||
                 hediff_Injury.Part.def.oldInjuryBaseChance == 0f ||
                 hediff_Injury.Part.def.IsSolid(hediff_Injury.Part, pawn.health.hediffSet.hediffs) ||
                 pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediff_Injury.Part)))
            {
                return;
            }
            FinalizeAndAddInjury(pawn, hediff_Injury, dInfo, ref result);
            CheckPropagateDamageToInnerSolidParts(dInfo, pawn, hediff_Injury, flag, ref result);
            CheckDuplicateDamageToOuterParts(dInfo, pawn, hediff_Injury, flag, ref result);
        }

        // flat damage reduction armor system and armor penetration
        public void CheckDuplicateDamageToOuterParts(DamageInfo dInfo, Pawn pawn, Hediff_Injury injury,
            bool involveArmor, ref LocalInjuryResult result)
        {
            if (!dInfo.AllowDamagePropagation)
            {
                return;
            }
            if (dInfo.Def.harmAllLayersUntilOutside && injury.Part.depth == BodyPartDepth.Inside)
            {
                var parent = injury.Part.parent;
                do
                {
                    if (pawn.health.hediffSet.GetPartHealth(parent) != 0f)
                    {
                        var hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dInfo.Def, pawn, parent);
                        var hediff_Injury = (Hediff_Injury) HediffMaker.MakeHediff(hediffDefFromDamage, pawn);
                        hediff_Injury.Part = parent;
                        hediff_Injury.source = injury.source;
                        hediff_Injury.sourceBodyPartGroup = injury.sourceBodyPartGroup;
                        hediff_Injury.Severity = dInfo.Amount;
                        if (involveArmor)
                        {
                            var dInfoLocal = new DamageInfo(dInfo);
                            hediff_Injury.Severity = GetAfterArmorDamage(pawn, dInfoLocal, parent);
                        }
                        if (hediff_Injury.Severity <= 0f)
                        {
                            hediff_Injury.Severity = 1f;
                        }
                        result.lastHitPart = hediff_Injury.Part;
                        FinalizeAndAddInjury(pawn, hediff_Injury, dInfo, ref result);
                    }
                    if (parent.depth == BodyPartDepth.Outside)
                    {
                        break;
                    }
                    parent = parent.parent;
                } while (parent != null);
            }
        }

        // flat damage reduction armor system and armor penetration
        public void CheckPropagateDamageToInnerSolidParts(DamageInfo dInfo, Pawn pawn, Hediff_Injury injury,
            bool involveArmor, ref LocalInjuryResult result)
        {
            if (!dInfo.AllowDamagePropagation)
            {
                return;
            }
            if (Rand.Value >= HealthTunings.ChanceToAdditionallyDamageInnerSolidPart)
            {
                return;
            }
            if (dInfo.Def.hasChanceToAdditionallyDamageInnerSolidParts &&
                !injury.Part.def.IsSolid(injury.Part, pawn.health.hediffSet.hediffs) &&
                injury.Part.depth == BodyPartDepth.Outside)
            {
                var source = from x in pawn.health.hediffSet.GetNotMissingParts(null, null)
                    where
                        x.parent == injury.Part && x.def.IsSolid(x, pawn.health.hediffSet.hediffs) &&
                        x.depth == BodyPartDepth.Inside
                    select x;
                BodyPartRecord part;
                if (source.TryRandomElementByWeight(x => x.absoluteFleshCoverage, out part))
                {
                    var hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dInfo.Def, pawn, part);
                    var hediff_Injury = (Hediff_Injury) HediffMaker.MakeHediff(hediffDefFromDamage, pawn);
                    hediff_Injury.Part = part;
                    hediff_Injury.source = injury.source;
                    hediff_Injury.sourceBodyPartGroup = injury.sourceBodyPartGroup;
                    hediff_Injury.Severity = dInfo.Amount/2;
                    if (involveArmor)
                    {
                        var dInfoLocal = new DamageInfo(dInfo);
                        dInfoLocal.SetAmount(dInfo.Amount / 2);
                        hediff_Injury.Severity = GetAfterArmorDamage(pawn, dInfoLocal, part);
                    }
                    if (hediff_Injury.Severity <= 0f)
                    {
                        return;
                    }
                    result.lastHitPart = hediff_Injury.Part;
                    FinalizeAndAddInjury(pawn, hediff_Injury, dInfo, ref result);
                }
            }
        }

        public static BodyPartRecord GetExactPartFromDamageInfo(DamageInfo dInfo, Pawn pawn)
        {
            if (dInfo.Part?.Part == null)
            {
                var randomNotMissingPart = pawn.health.hediffSet.GetRandomNotMissingPart(dInfo.Def, dInfo.Part?.Height, dInfo.Part?.Depth);
                if (randomNotMissingPart == null)
                {
                    Log.Warning("GetRandomNotMissingPart returned null (any part).");
                }
                return randomNotMissingPart;
            }
            if (!dInfo.Part.Value.CanMissBodyPart)
            {
                return (from x in pawn.health.hediffSet.GetNotMissingParts(null, null)
                    where x == dInfo.Part.Value.Part
                    select x).FirstOrDefault();
            }
            var randomNotMissingPart2 = pawn.health.hediffSet.GetRandomNotMissingPart(null, null);
            if (randomNotMissingPart2 == null)
            {
                Log.Warning("GetRandomNotMissingPart returned null (specified part).");
            }
            return randomNotMissingPart2;
        }

        public void FinalizeAndAddInjury(Pawn pawn, Hediff_Injury injury, DamageInfo dInfo, ref LocalInjuryResult result)
        {
            CalculateOldInjuryDamageThreshold(pawn, injury);
            result.totalDamageDealt += Mathf.Min(injury.Severity, pawn.health.hediffSet.GetPartHealth(injury.Part));
            pawn.health.AddHediff(injury, null, dInfo);
        }

        public static bool IsHeadshot(DamageInfo dInfo, Hediff_Injury injury)
        {
            return !dInfo.InstantOldInjury && injury.Part.groups.Contains(BodyPartGroupDefOf.FullHead) &&
                   dInfo.Def == DamageDefOf.Bullet;
        }

        public void CalculateOldInjuryDamageThreshold(Pawn pawn, Hediff_Injury injury)
        {
            var hediffCompProperties = injury.def.CompPropsFor(typeof (HediffComp_GetsOld));
            if (hediffCompProperties == null)
            {
                return;
            }
            if (injury.Part.def.IsSolid(injury.Part, pawn.health.hediffSet.hediffs) ||
                pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(injury.Part) || injury.IsOld() ||
                injury.Part.def.oldInjuryBaseChance < 1E-05f)
            {
                return;
            }
            var isDelicate = injury.Part.def.IsDelicate;
            if ((Rand.Value <= injury.Part.def.oldInjuryBaseChance*hediffCompProperties.becomeOldChance &&
                 injury.Severity >= injury.Part.def.GetMaxHealth(pawn)*0.25f && injury.Severity >= 7f) || isDelicate)
            {
                var hediffComp_GetsOld = injury.TryGetComp<HediffComp_GetsOld>();
                var num = 1f;
                var num2 = injury.Severity/2f;
                if (num <= num2)
                {
                    hediffComp_GetsOld.oldDamageThreshold = Rand.Range(num, num2);
                }
                if (isDelicate)
                {
                    hediffComp_GetsOld.oldDamageThreshold = injury.Severity;
                    hediffComp_GetsOld.IsOld = true;
                }
            }
        }
    }
}
