using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public static class RA_Motes
    {
        public static void ThrowSmoke(Vector3 loc, float size, string moteDefName)
        {
            if (!loc.ShouldSpawnMotesAt()) return;

            var moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke);
            moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
            moteThrown.rotationRate = Rand.Range(-30f, 30f);
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocity(Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
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

        public static void ThrowSmokeTrail(Vector3 loc, float size)
        {
            if (Find.TickManager.TicksGame % 2 == 0)
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
