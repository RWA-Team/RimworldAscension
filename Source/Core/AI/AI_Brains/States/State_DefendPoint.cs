using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.SquadAI;

namespace RA
{
    internal class State_DefendPoint : State
    {
        public IntVec3 defendPoint;

        public float defendRadius = 28f;

        public override IntVec3 FlagLoc
        {
            get
            {
                // return new IntVec3(defendPointTrader.x, defendPointTrader.y, defendPointTrader.z - 2);
                return this.defendPoint;
            }
        }

        public State_DefendPoint()
        {
        }

        public State_DefendPoint(IntVec3 defendPoint)
        {
            this.defendPoint = defendPoint;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<IntVec3>(ref this.defendPoint, "defendPoint", default(IntVec3), false);
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.brain.ownedPawns.Count; i++)
            {
                this.brain.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Defend, this.defendPoint, -1f);
                this.brain.ownedPawns[i].mindState.duty.focusSecond = this.defendPoint;
                this.brain.ownedPawns[i].mindState.duty.radius = this.defendRadius;
            }
        }

        public override void StateTick()
        {
            base.StateTick();
        }
    }
}