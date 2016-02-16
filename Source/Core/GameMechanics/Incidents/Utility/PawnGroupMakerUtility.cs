using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;


namespace RimWorld
{
    public class PawnGroupMakerUtility
    {
        public static IEnumerable<Pawn> GenerateArrivingPawns(IncidentParms parms, bool warnOnZeroResults = true)
        {
            //Adjust points for raid strategy (if it is a raid; it may not be)
            if (parms.raidStrategy != null)
                parms.points *= parms.raidStrategy.pointsFactor;

            //Adjust points for raid arrival mode
            switch (parms.raidArrivalMode)
            {
                case PawnsArriveMode.EdgeWalkIn: parms.points *= 1.0f; break;
                case PawnsArriveMode.EdgeDrop: parms.points *= 1.0f; break;
                case PawnsArriveMode.CenterDrop: parms.points *= 0.45f; break;
            }

            //This can happen now if our arrival mode/style has modified our points below
            //the threshold where we can make any pawns. In this case, just give us some points
            //to play with to make something at least.
            parms.points = Mathf.Max(parms.points, parms.faction.def.MinPointsToGeneratePawnGroup() * 1.05f);

            //Choose a PawnGroupMaker
            PawnGroupMaker chosenGroupMaker;
            var usableGroupMakers = parms.faction.def.pawnGroupMakers.Where(gm => gm.CanGenerateFrom(parms));
            if (!usableGroupMakers.TryRandomElementByWeight(gm => gm.commonality, out chosenGroupMaker))
            {
                Log.Error("Faction " + parms.faction + " of def " + parms.faction.def + " has no PawnGroupMakers that can generate for parms " + parms);
                yield break;
            }

            //Generate pawns from that GroupMaker
            foreach (var p in chosenGroupMaker.GenerateArrivingPawns(parms, warnOnZeroResults))
            {
                yield return p;
            }
        }

        public static void LogPawnGroupsMade()
        {
            foreach (var fac in Find.FactionManager.AllFactions)
            {
                if (fac.def.pawnGroupMakers.NullOrEmpty())
                    continue;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("======== FACTION: " + fac.name + " (" + fac.def.defName + ") min=" + fac.def.MinPointsToGeneratePawnGroup() + " =======");

                Action<float> addLog = points =>
                {
                    if (points < fac.def.MinPointsToGeneratePawnGroup())
                        return;

                    IncidentParms parms = new IncidentParms();
                    parms.points = points;
                    sb.AppendLine("Group with " + parms.points + " points");
                    float totalCost = 0;
                    foreach (var p in GenerateArrivingPawns(parms, false))
                    {
                        string eqName;
                        if (p.equipment.Primary != null)
                            eqName = p.equipment.Primary.Label;
                        else
                            eqName = "NoEquipment";

                        sb.AppendLine("  " + p.kindDef.combatPower.ToString("F0").PadRight(5) + "- " + p.kindDef.defName + ", " + eqName);
                        totalCost += p.kindDef.combatPower;
                    }
                    sb.AppendLine("         totalCost " + totalCost);
                    sb.AppendLine();
                };

                addLog(35);
                addLog(70);
                addLog(135);
                addLog(200);
                addLog(300);
                addLog(500);
                addLog(800);
                addLog(1200);
                addLog(2000);
                addLog(3000);
                addLog(4000);

                Log.Message(sb.ToString());
            }
        }
    }


}