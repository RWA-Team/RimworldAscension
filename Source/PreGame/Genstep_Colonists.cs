using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RimworldAscension.PreGame
{
    public class Genstep_Colonists : Genstep
    {
        private const int NumStartingMealsPerColonist = 10;

        private const int NumStartingMedPacksPerColonist = 6;

        public override void Generate()
        {
            foreach (Pawn current in MapInitData.colonists)
            {
                current.SetFactionDirect(Faction.OfColony);
                PawnUtility.AddAndRemoveComponentsAsAppropriate(current);
                current.needs.mood.thoughts.TryGainThought(ThoughtDefOf.NewColonyOptimism);
            }
            Genstep_Colonists.CreateInitialWorkSettings();
            bool startedDirectInEditor = MapInitData.StartedDirectInEditor;
            List<List<Thing>> list = new List<List<Thing>>();
            foreach (Pawn current2 in MapInitData.colonists)
            {
                if (MapInitData.startedFromEntry && Rand.Value < 0.5f)
                {
                    current2.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null);
                }
                List<Thing> list2 = new List<Thing>();
                list2.Add(current2);
                Thing thing = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack, null);
                thing.stackCount = NumStartingMealsPerColonist;
                list2.Add(thing);
                Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Medicine, null);
                thing2.stackCount = NumStartingMedPacksPerColonist;
                list2.Add(thing2);
                list.Add(list2);
            }
            bool canInstaDropDuringInit = startedDirectInEditor;
            DropPodUtility.DropThingGroupsNear(MapGenerator.PlayerStartSpot, list, 110, canInstaDropDuringInit, true, true);
        }

        private static Thing RandomPet()
        {
            PawnKindDef kindDef = (from td in DefDatabase<PawnKindDef>.AllDefs
                                   where td.race.category == ThingCategory.Pawn && td.RaceProps.petness > 0f
                                   select td).RandomElementByWeight((PawnKindDef td) => td.RaceProps.petness);
            Pawn pawn = PawnGenerator.GeneratePawn(kindDef, Faction.OfColony, false, 0);
            if (pawn.Name == null || pawn.Name.Numerical)
            {
                pawn.Name = NameGenerator.GenerateName(pawn, NameStyle.Full);
            }
            return pawn;
        }

        private static void CreateInitialWorkSettings()
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
