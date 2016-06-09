using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public static class RA_Motes
    {
        public static void ThrowSmoke(Vector3 loc, float size, string moteDefName)
        {
            var moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named(moteDefName));
            moteThrown.ScaleUniform = Rand.Range(1.5f, 2.5f) * size;
            moteThrown.exactRotationRate = Rand.Range(-0.5f, 0.5f);
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocityAngleSpeed(Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
        }

        public static void ThrowSmokeBlack(Vector3 loc, float size)
        {
            // Only throw smoke every 10 ticks
            if (Find.TickManager.TicksGame % 10 == 0)
            {
                ThrowSmoke(loc, size, "Mote_SmokeBlack");
            }
        }

        public static void ThrowSmokeWhite(Vector3 loc, float size)
        {
            // Only throw smoke every 10 ticks
            if (Find.TickManager.TicksGame % 10 == 0)
            {
                ThrowSmoke(loc, size, "Mote_SmokeWhite");
            }
        }

        public static void ThrowSmokeBlack_Signal(Vector3 loc, float size)
        {
            // throw smoke for a second period each 6 seconds
            if (Find.TickManager.TicksGame % 2 == 0 && (Find.TickManager.TicksGame % (GenDate.SecondsToTicks(1) * 7) < GenDate.SecondsToTicks(1)))
            {
                ThrowSmoke(loc, size, "Mote_SmokeBlack_Signal");
            }
        }
    }
}
