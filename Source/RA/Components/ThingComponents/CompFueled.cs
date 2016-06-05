using UnityEngine;
using Verse;

namespace RA
{
    public class CompFueled : ThingComp
    {
        public CompFueled_Properties Properties => (CompFueled_Properties)props;
    }

    public class CompFueled_Properties : CompProperties
    {
        public Vector3 fireDrawOffset = Vector3.zero;
        public Vector3 smokeDrawOffset = Vector3.zero;
        public Vector3 fuelDrawOffset = Vector3.zero;
        public float fuelDrawScale = 1f;
        public float fireDrawScale = 1f;
        public float operatingTemp = 1000f;

        public CompFueled_Properties()
        {
            compClass = typeof(CompFueled);
        }
    }
}