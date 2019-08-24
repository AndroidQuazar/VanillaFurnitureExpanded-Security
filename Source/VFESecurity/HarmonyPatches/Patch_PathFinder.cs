using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Harmony;

namespace VFESecurity
{

    public static class Patch_PathFinder
    {

        [HarmonyPatch(typeof(PathFinder), nameof(PathFinder.FindPath), new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode) })]
        public static class FindPath
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                bool done = false;

                var adjustedTerrainCostInfo = AccessTools.Method(typeof(FindPath), nameof(AdjustedTerrainCost));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Look for the section that checks the terrain grid pathCosts
                    if (!done && instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder lb && lb.LocalIndex == 49)
                    {
                        var secondInstructionBehind = instructionList[i - 2];
                        if (secondInstructionBehind.opcode == OpCodes.Ldelem_I4)
                        {
                            yield return instruction; // num17 += array[num15]
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 49); // num17
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 45); // num15
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5); // num
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 14); // topGrid
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
                        var nextTerrainDefExtension = nextTerrain.GetModExtension<TerrainDefExtension>() ?? TerrainDefExtension.defaultValues;
                        if (nextTerrainDefExtension.pathCostEntering > -1)
                        {
                            cost += (nextTerrainDefExtension.pathCostEntering - nextTerrain.pathCost);
                        }

                        var curTerrainDefExtension = curTerrain.GetModExtension<TerrainDefExtension>() ?? TerrainDefExtension.defaultValues;
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
