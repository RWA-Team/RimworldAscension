using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using static System.String;

namespace RA
{
    public static class ArmorUtil
    {
        // flat damage reduction armor system and armor penetration
        public static int GetAfterArmorDamage(Pawn pawn, DamageInfo dInfo, BodyPartRecord bPart)
        {
            Log.Message(Format("BodyPart hit = {0}", bPart.def.LabelCap));

            var armorStat = dInfo.Def.armorCategory.DeflectionStat();

            var remainingDamage = (float) dInfo.Amount;
            if (dInfo.Def.armorCategory == DamageArmorCategory.IgnoreArmor)
            {
                return dInfo.Amount;
            }

            var armorPenetration = 0f;
            if (dInfo.Instigator != null)
            {
                // check if damage is cause by projectile
                var projectile = dInfo.Source?.Verbs?.FirstOrDefault(verb => verb.projectileDef != null)?.projectileDef;

                armorPenetration = projectile?.GetStatValueAbstract(StatDef.Named("ArmorPenetration")) ?? ((dInfo.Instigator as Pawn)?.equipment?.Primary?.GetStatValue(StatDef.Named("ArmorPenetration")) ?? 0f);
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

            return Mathf.RoundToInt(remainingDamage);
        }

        public static void ApplyDamageReduction(ref float DMG, ref float AP, float AR,
            Thing armorThing, DamageInfo dInfo)
        {
            Log.Message(Format("Initial AR of {0} = {1}", armorThing, AR));
            Log.Message(Format("AP of {0} = {1}", dInfo.Source, AP));
            // limit armor value after AP is applied to 0
            AR = Mathf.Clamp(AR - AP, 0, AR);
            Log.Message(Format("Actual AR after AP applied = {0}", AR));
            var blockedDamage = Mathf.Min(DMG, AR);
            Log.Message(Format("Initial DMG = {0}", DMG));
            Log.Message(Format("Blocked DMG = {0}", blockedDamage));
            armorThing?.TakeDamage(new DamageInfo(dInfo.Def, Mathf.RoundToInt(blockedDamage), null, null));
            DMG -= blockedDamage;
            Log.Message(Format("Result DMG = {0}", DMG));
        }
    }
}
