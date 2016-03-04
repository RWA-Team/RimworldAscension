using System;
using UnityEngine;
using Verse;

namespace RA
{
    public class CompShaker : ThingComp
    {
        public const float ShakeDecayRate = 0.5f;
        public const float ShakeFrequency = 24f;
        public const float MaxShakeMag = 1f;
        public static float curShakeMag;

        public static Vector3 ShakeOffset
        {
            get
            {
                var x = (float)Math.Sin(Find.RealTime.timeUnpaused * ShakeFrequency) * curShakeMag;
                var y = (float)Math.Sin(Find.RealTime.timeUnpaused * ShakeFrequency * 1.05) * curShakeMag;
                var z = (float)Math.Sin(Find.RealTime.timeUnpaused * ShakeFrequency * 1.1) * curShakeMag;
                return new Vector3(x, y, z);
            }
        }

        public static void DoShake(float mag)
        {
            curShakeMag += mag;
            if (curShakeMag > MaxShakeMag)
            {
                curShakeMag = MaxShakeMag;
            }

            curShakeMag -= ShakeDecayRate * Time.deltaTime;
            if (curShakeMag < 0f)
            {
                curShakeMag = 0f;
            }

            Find.CameraMap.JumpTo(Find.CameraMap.MapPosition.ToVector3Shifted() + ShakeOffset);
        }
    }
}
