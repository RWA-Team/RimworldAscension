using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    public class RA_GameEnder
    {
        public const int GraceTicksBeforeGameEndCheck = 1000;
        public bool gameEnding;
        public int ticksToGameOver = -1;

        public void CheckGameOver()
        {
            if (Find.TickManager.TicksGame < GraceTicksBeforeGameEndCheck)
            {
                return;
            }
            if (gameEnding)
            {
                return;
            }
            foreach (var current in Find.MapPawns.FreeColonists.Where(current => !current.Destroyed))
            {
                if (current.holder == null)
                {
                    return;
                }
                var building_CryptosleepCasket =
                    current.holder.owner as Building_CryptosleepCasket;
                if (building_CryptosleepCasket != null &&
                    building_CryptosleepCasket.def.building.isPlayerEjectable)
                {
                    return;
                }
            }
            gameEnding = true;
            ticksToGameOver = 400;
        }

        public void GameEndTick()
        {
            if (gameEnding)
            {
                ticksToGameOver--;
                if (ticksToGameOver == 0)
                {
                    GenGameEnd.EndGameDialogMessage("GameOverEveryoneDead".Translate());
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref gameEnding, "gameEnding", false);
            Scribe_Values.LookValue(ref ticksToGameOver, "ticksToGameOver", -1);
        }
    }
}
