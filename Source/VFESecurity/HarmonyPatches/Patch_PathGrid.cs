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

    public static class Patch_PathGrid
    {

        [HarmonyPatch(typeof(PathGrid), nameof(PathGrid.CalculatedCostAt))]
        public static class CalculatedCostAt
        {

            [HarmonyPriority(Priority.First)]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var defInfo = AccessTools.Field(typeof(Thing), nameof(Thing.def));
                
                var pathCostInfo = AccessTools.Field(typeof(BuildableDef), nameof(BuildableDef.pathCost));

                var finalPathCostInfo = AccessTools.Method(typeof(CalculatedCostAt), nameof(FinalPathCost));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Add our helper method call to each reference to pathCost from a ThingDef
                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == pathCostInfo)
                    {
                        var prevInstruction = instructionList[i - 1];
                        if (prevInstruction.opcode == OpCodes.Ldfld && prevInstruction.operand == defInfo)
                        {
                            yield return instruction; // thing.def.pathCost
                            yield return instructionList[i - 2].Clone(); // thing
                            instruction = new CodeInstruction(OpCodes.Call, finalPathCostInfo); // FinalPathCost(thing.def.pathCost, thing)
                        }
                    }

                    yield return instruction;
                }
            }

            private static int FinalPathCost(int original, Thing t)
            {
                if (t.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
                    return retractableComp.Props.submergedPathCost;
                return original;
            }

        }


    }

}
