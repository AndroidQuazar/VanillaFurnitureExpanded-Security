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
using HarmonyLib;

namespace VFESecurity
{

    public static class Patch_VerbProperties
    {

        [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.DrawRadiusRing))]
        public static class DrawRadiusRing
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("Transpiler start: VerbProperties.DrawRadiusRing (1 match)");
                #endif

                var instructionList = instructions.ToList();

                var rangeInfo = AccessTools.Field(typeof(VerbProperties), nameof(VerbProperties.range));
                var minRangeInfo = AccessTools.Field(typeof(VerbProperties), nameof(VerbProperties.minRange));

                var adjustedRangeInfo = AccessTools.Method(typeof(TrenchUtility), nameof(TrenchUtility.FinalAdjustedRangeFromTerrain), new Type[] { typeof(float), typeof(float), typeof(IntVec3) });

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(rangeInfo))
                    {
                        #if DEBUG
                            Log.Message("VerbProperties.DrawRadiusRing match 1 of 1");
                        #endif

                        yield return instruction; // this.range
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        yield return new CodeInstruction(OpCodes.Ldfld, minRangeInfo); // this.minRange
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // center
                        instruction = new CodeInstruction(OpCodes.Call, adjustedRangeInfo); // TrenchUtility.FinalAdjustedRangeFromTerrain(this.range, this.minRange, center)
                    }

                    yield return instruction;
                }
            }

        }
        

    }

}
