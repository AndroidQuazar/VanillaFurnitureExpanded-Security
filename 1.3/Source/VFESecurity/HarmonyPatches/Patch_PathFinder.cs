/*#define DEBUG*/
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace VFESecurity
{
    public static class Patch_PathFinder
    {
        [HarmonyPatch(typeof(PathFinder), nameof(PathFinder.FindPath), new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode), typeof(PathFinderCostTuning) })]
        public static class FindPath
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                    Log.Message("Transpiler start: PathFinder.FindPath (2 matches)");
#endif

                var instructionList = instructions.ToList();

                bool done = false;

                var adjustedTerrainCostInfo = AccessTools.Method(typeof(FindPath), nameof(AdjustedTerrainCost));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Look for the section that checks the terrain grid pathCosts
                    if (!done && instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder lb && lb.LocalIndex == 41)
                    {
#if DEBUG
                            Log.Message("PathFinder.FindPath match 1 of 2");
#endif

                        var secondInstructionBehind = instructionList[i - 2];
                        if (secondInstructionBehind.opcode == OpCodes.Ldelem_I4)
                        {
#if DEBUG
                                Log.Message("PathFinder.FindPath match 2 of 2");
#endif

                            yield return instruction; // num17 += array[num15]
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 41); // num17
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 38); // num15
                            yield return new CodeInstruction(OpCodes.Ldloc_3); // num
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 12); // topGrid
                            yield return new CodeInstruction(OpCodes.Ldarg_3); // parms
                            yield return new CodeInstruction(OpCodes.Call, adjustedTerrainCostInfo); // AdjustedTerrainCost(num17, num15, num, topGrid, parms)
                            instruction = instruction.Clone(); // num
                        }
                    }

                    yield return instruction;
                }
            }

            private static int AdjustedTerrainCost(int cost, int nextIndex, int curIndex, TerrainDef[] terrainGrid, TraverseParms parms)
            {
                if (parms.pawn != null)
                {
                    var curTerrain = terrainGrid[curIndex];
                    var nextTerrain = terrainGrid[nextIndex];
                    if (curTerrain != nextTerrain)
                    {
                        var nextTerrainDefExtension = TerrainDefExtension.Get(nextTerrain);
                        if (nextTerrainDefExtension.pathCostEntering > -1)
                        {
                            cost += (nextTerrainDefExtension.pathCostEntering - nextTerrain.pathCost);
                        }

                        var curTerrainDefExtension = TerrainDefExtension.Get(curTerrain);
                        if (curTerrainDefExtension.pathCostLeaving > -1)
                        {
                            cost += (curTerrainDefExtension.pathCostLeaving - curTerrain.pathCost);
                        }
                    }
                }
                return cost;
            }
        }
    }
}