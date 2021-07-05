using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace VFESecurity
{
    public static class Patch_PathGrid
    {
        [HarmonyPatch(typeof(PathGrid), nameof(PathGrid.CalculatedCostAt))]
        public static class CalculatedCostAt
        {
            [HarmonyPriority(Priority.First)]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                    Log.Message("Transpiler start: PathGrid.CalculatedCostAt (3 matches)");
#endif

                var instructionList = instructions.ToList();

                var defInfo = AccessTools.Field(typeof(Thing), nameof(Thing.def));

                var pathCostInfo = AccessTools.Field(typeof(BuildableDef), nameof(BuildableDef.pathCost));

                var mapInfo = AccessTools.Field(typeof(PathGrid), "map");

                var finalTerrainPathCostInfo = AccessTools.Method(typeof(CalculatedCostAt), nameof(FinalTerrainPathCost));
                var finalThingPathCostInfo = AccessTools.Method(typeof(CalculatedCostAt), nameof(FinalThingPathCost));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Add our helper method call to each reference to pathCost from...
                    if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(pathCostInfo))
                    {
#if DEBUG
                            Log.Message("PathGrid.CalculatedCostAt match 1 of 3");
#endif

                        var prevInstruction = instructionList[i - 1];

                        // ...a ThingDef
                        if (prevInstruction.opcode == OpCodes.Ldfld && prevInstruction.OperandIs(defInfo))
                        {
#if DEBUG
                                Log.Message("PathGrid.CalculatedCostAt match 2 of 3");
#endif

                            yield return instruction; // thing.def.pathCost
                            yield return instructionList[i - 2].Clone(); // thing
                            instruction = new CodeInstruction(OpCodes.Call, finalThingPathCostInfo); // FinalPathCost(thing.def.pathCost, thing)
                        }

                        // ...a TerrainDef
                        if (prevInstruction.opcode == OpCodes.Ldloc_2)
                        {
#if DEBUG
                                Log.Message("PathGrid.CalculatedCostAt match 3 of 3");
#endif

                            yield return instruction; // terrainDef.pathCost
                            yield return prevInstruction.Clone(); // terrain
                            yield return new CodeInstruction(OpCodes.Ldarg_3); // prevCell
                            yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                            yield return new CodeInstruction(OpCodes.Ldfld, mapInfo); // this.map
                            instruction = new CodeInstruction(OpCodes.Call, finalTerrainPathCostInfo); // FinalTerrainPathCost(terrainDef.pathCost, terrain, prevCell, this.map)
                        }
                    }

                    yield return instruction;
                }
            }

            private static int FinalTerrainPathCost(int original, TerrainDef terrain, IntVec3 prevCell, Map map)
            {
                if (prevCell.IsValid)
                {
                    var prevTerrain = map.terrainGrid.TerrainAt(prevCell);
                    if (terrain != prevTerrain)
                    {
                        // Entering terrain
                        var terrainDefExtension = TerrainDefExtension.Get(terrain);
                        if (terrainDefExtension.pathCostEntering > -1)
                            return terrainDefExtension.pathCostEntering;

                        // Exiting terrain
                        var prevTerrainDefExtension = TerrainDefExtension.Get(prevTerrain);
                        if (prevTerrainDefExtension.pathCostLeaving > -1)
                            return prevTerrainDefExtension.pathCostLeaving;
                    }
                }
                return original;
            }

            private static int FinalThingPathCost(int original, Thing t)
            {
                if (t.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
                    return retractableComp.Props.submergedPathCost;
                return original;
            }
        }
    }
}