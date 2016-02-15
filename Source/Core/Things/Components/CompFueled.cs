using UnityEngine;
using Verse;

namespace RA
{
    public class CompFueled : ThingComp
    {
        public CompFueled_Properties Properties
        {
            get
            {
                return (CompFueled_Properties)props;
            }
        }
    }

    public class CompFueled_Properties : CompProperties
    {
        public Vector3 fireDrawOffset = new Vector3(0f, 0f, 0f);
        public Vector3 smokeDrawOffset = new Vector3(0f, 0f, 0f);
        public Vector3 fuelDrawOffset = new Vector3(0f, 0f, 0f);
        public float fuelDrawScale = 1f;
        public float fireDrawScale = 1f;
        public float operatingTemp = 1000f;

        // Default requirement
        public CompFueled_Properties()
        {
            this.compClass = typeof(CompFueled_Properties);
        }
    }
}

