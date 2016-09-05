using UnityEngine;
using Verse;

namespace RA
{
    // hands are draw only if any position shift from Vector3.zero is set, like (0, 0, 0.1f)
    public class CompWeaponExtensions : ThingComp
    {
        public Vector3 FirstHandPosition => (props as CompWeaponExtensions_Properties).firstHandPosition;
        public Vector3 SecondHandPosition => (props as CompWeaponExtensions_Properties).secondHandPosition;
        public Vector3 WeaponPositionOffset => (props as CompWeaponExtensions_Properties).weaponPositionOffset;
        public float AttackAngleOffset => (props as CompWeaponExtensions_Properties).attackAngleOffset;
    }

    public class CompWeaponExtensions_Properties : CompProperties
    {
        public Vector3 firstHandPosition = Vector3.zero;
        public Vector3 secondHandPosition = Vector3.zero;
        public Vector3 weaponPositionOffset = Vector3.zero;
        public int attackAngleOffset = 0;

        public CompWeaponExtensions_Properties()
        {
            compClass = typeof (CompWeaponExtensions);
        }
    }
}