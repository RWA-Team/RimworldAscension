using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public static class ArmorUtil
    {
        // flat damage reduction armor system and armor penetration
        public static int GetAfterArmorDamage(Pawn pawn, DamageInfo dInfo, BodyPartRecord bPart)
        {
            Log.Message("base damage " + dInfo.Amount);

            var armorStat = dInfo.Def.armorCategory.DeflectionStat();

            var remainingDamage = (float) dInfo.Amount;
            if (dInfo.Def.armorCategory == DamageArmorCategory.IgnoreArmor)
            {
                return dInfo.Amount;
            }

            var armorPenetration = 0f;
            if (dInfo.Instigator != null)
            {
                armorPenetration =
                    (dInfo.Instigator as Pawn)?.equipment?.Primary?.GetStatValue(StatDef.Named("ArmorPenetration")) ??
                    dInfo.Instigator.GetStatValue(StatDef.Named("ArmorPenetration"));
            }

            if (pawn.apparel != null)
            {
                var wornApparel = pawn.apparel.WornApparel;
                foreach (var apparel in wornApparel.Where(apparel => apparel.def.apparel.CoversBodyPart(bPart)))
                {
                    ApplyDamageReduction(ref remainingDamage, ref armorPenetration, apparel.GetStatValue(armorStat),
                        apparel, dInfo);
                    if (remainingDamage < 0.01f)
                    {
                        return 0;
                    }
                }
            }

            ApplyDamageReduction(ref remainingDamage, ref armorPenetration, pawn.GetStatValue(armorStat), null, dInfo);

            Log.Message("final damage " + Mathf.RoundToInt(remainingDamage));

            return Mathf.RoundToInt(remainingDamage);
        }

        public static void ApplyDamageReduction(ref float damageAmount, ref float penetrationAmount, float armorValue,
            Thing armorThing, DamageInfo dInfo)
        {
            Log.Message("base armor " + armorValue);
            Log.Message("penetrationAmount " + penetrationAmount);
            // limit armor value after AP is applied to 0
            armorValue = Mathf.Clamp(armorValue - penetrationAmount, 0, armorValue);
            Log.Message("final armor " + armorValue);
            var blockedDamage = Mathf.Min(damageAmount, armorValue);
            Log.Message("blockedDamage " + blockedDamage);
            armorThing?.TakeDamage(new DamageInfo(dInfo.Def, Mathf.RoundToInt(blockedDamage), null, null));
            damageAmount -= blockedDamage;
        }
    }
}
