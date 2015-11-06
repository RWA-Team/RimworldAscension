using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
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
                float x = (float)Math.Sin((double)(Find.RealTime.timeUnpaused * ShakeFrequency)) * curShakeMag;
                float y = (float)Math.Sin((double)(Find.RealTime.timeUnpaused * ShakeFrequency) * 1.05) * curShakeMag;
                float z = (float)Math.Sin((double)(Find.RealTime.timeUnpaused * ShakeFrequency) * 1.1) * curShakeMag;
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
