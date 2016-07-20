using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RA
{
    // vanilla class as it is, to keep the required references in RA_DefGenerator
    public static class TerrainDefGenerator_Stone
    {
        public static IEnumerable<TerrainDef> ImpliedTerrainDefs()
        {
            var i = 0;
            foreach (var rock in DefDatabase<ThingDef>.AllDefs.Where(def => def.building != null
                                                                            && def.building.isNaturalRock
                                                                            && !def.building.isResourceRock))
            {
                var rough = new TerrainDef();
                var hewn = new TerrainDef();
                var smooth = new TerrainDef();

                //Rough stone
                {
                    //Set values that are the same for all rough stones
                    rough.texturePath = "Terrain/Surfaces/RoughStone";
                    rough.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
                    rough.pathCost = 1;
                    StatUtility.SetStatValueInList(ref rough.statBases, StatDefOf.Beauty, -1);
                    rough.scatterType = "Rocky";
                    rough.affordances = new List<TerrainAffordance>
                    {
                        TerrainAffordance.Light,
                        TerrainAffordance.Heavy,
                        TerrainAffordance.SmoothableStone
                    };
                    rough.fertility = 0;

                    //Set values specific to this rock type
                    rough.renderPrecedence = 190 + i;
                    rough.defName = rock.defName + "_Rough";
                    rough.label = "RoughStoneTerrainLabel".Translate(rock.label);
                    rough.description = "RoughStoneTerrainDesc".Translate(rock.label);
                    rough.color = rock.graphicData.color;

                    //Link rock back to me
                    rock.naturalTerrain = rough;
                }

                //Rough-hewn rock
                {
                    //Set values that are the same for all rough stones
                    hewn.texturePath = "Terrain/Surfaces/RoughHewnRock";
                    hewn.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
                    hewn.pathCost = 1;
                    StatUtility.SetStatValueInList(ref hewn.statBases, StatDefOf.Beauty, -1);
                    hewn.scatterType = "Rocky";
                    hewn.affordances = new List<TerrainAffordance>
                    {
                        TerrainAffordance.Light,
                        TerrainAffordance.Heavy,
                        TerrainAffordance.SmoothableStone
                    };
                    hewn.fertility = 0;

                    //Set values specific to this rock type
                    hewn.renderPrecedence = 50 + i;
                    hewn.defName = rock.defName + "_RoughHewn";
                    hewn.label = "RoughHewnStoneTerrainLabel".Translate(rock.label);
                    hewn.description = "RoughHewnStoneTerrainDesc".Translate(rock.label);
                    hewn.color = rock.graphicData.color;

                    //Link rock back to me
                    rock.leaveTerrain = hewn;
                }

                //Smoothed stone
                {
                    //Set values that are the same for all rough stones
                    smooth.texturePath = "Terrain/Surfaces/SmoothStone";
                    smooth.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
                    smooth.pathCost = 0;
                    StatUtility.SetStatValueInList(ref smooth.statBases, StatDefOf.Beauty, 3);
                    smooth.scatterType = "Rocky";
                    smooth.affordances = new List<TerrainAffordance>
                    {
                        TerrainAffordance.Light,
                        TerrainAffordance.Heavy,
                        TerrainAffordance.SmoothHard
                    };
                    smooth.fertility = 0;
                    smooth.acceptTerrainSourceFilth = true;

                    //Set values specific to this
                    smooth.renderPrecedence = 140 + i;
                    smooth.defName = rock.defName + "_Smooth";
                    smooth.label = "SmoothStoneTerrainLabel".Translate(rock.label);
                    smooth.description = "SmoothStoneTerrainDesc".Translate(rock.label);
                    smooth.color = rock.graphicData.color;

                    //Point the rough versions at the smooth version
                    rough.smoothedTerrain = smooth;
                    hewn.smoothedTerrain = smooth;
                }

                yield return rough;
                yield return hewn;
                yield return smooth;

                i++;
            }
        }
    }
}