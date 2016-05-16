using Verse;
using Verse.AI.Group;

namespace RA
{
    public class LordToilData_CaravanDefence : LordToilData
    {
        public int combatTimer;
        public int initialLordToilIndex;

        public LordToilData_CaravanDefence()
        {
        }

        public LordToilData_CaravanDefence(int combatTimer)
        {
            this.combatTimer = combatTimer;
        }

        public override void ExposeData()
        {
            Scribe_Values.LookValue(ref combatTimer, "combatTimer");
            Scribe_Values.LookValue(ref initialLordToilIndex, "initialLordToilIndex");
        }
    }
}