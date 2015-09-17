using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace GlobalMod
{
    public class Genstep_Colonists : Genstep
    {
        public int StartingColonistCount = 5;
        
        public override void Generate()
        {
            GenerateStartingColonists();

            foreach (Pawn current in MapInitData.colonists)
            {
                current.SetFactionDirect(Faction.OfColony);
                PawnUtility.AddAndRemoveComponentsAsAppropriate(current);
                current.needs.mood.thoughts.TryGainThought(ThoughtDefOf.NewColonyOptimism);
            }
            CreateInitialWorkSettings();
            
            foreach (Thing current in MapInitData.colonists)
            {
                GenPlace.TryPlaceThing(current, MapGenerator.PlayerStartSpot, ThingPlaceMode.Near);
            }
        }
        
        public void GenerateStartingColonists()
        {
            do
            {
                MapInitData.colonists.Clear();
                for (int i = 0; i < StartingColonistCount; i++)
                {
                    MapInitData.colonists.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfColony, false, 0));
                }
            }
            while (!MapInitData.AnyoneCanDoRequiredWorks());
        }

        public void CreateInitialWorkSettings()
        {
            foreach (Pawn current in MapInitData.colonists)
            {
                current.workSettings.DisableAll();
            }
            foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (w.alwaysStartActive)
                {
                    foreach (Pawn current2 in from col in MapInitData.colonists
                                              where !col.story.WorkTypeIsDisabled(w)
                                              select col)
                    {
                        current2.workSettings.SetPriority(w, 3);
                    }
                }
                else
                {
                    bool flag = false;
                    foreach (Pawn current3 in MapInitData.colonists)
                    {
                        if (!current3.story.WorkTypeIsDisabled(w) && current3.skills.AverageOfRelevantSkillsFor(w) >= 6f)
                        {
                            current3.workSettings.SetPriority(w, 3);
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        IEnumerable<Pawn> source = from col in MapInitData.colonists
                                                   where !col.story.WorkTypeIsDisabled(w)
                                                   select col;
                        if (source.Any<Pawn>())
                        {
                            Pawn pawn = source.InRandomOrder(null).MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(w));
                            pawn.workSettings.SetPriority(w, 3);
                        }
                        else if (w.requireCapableColonist)
                        {
                            Log.Error("No colonist could do requireCapableColonist work type " + w);
                        }
                    }
                }
            }
        }
    }
}
