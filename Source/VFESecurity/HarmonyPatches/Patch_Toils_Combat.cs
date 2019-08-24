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

    public static class Patch_Toils_Combat
    {

        public static class manual_GoToCastPosition_initAction
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var rangeInfo = AccessTools.Field(typeof(VerbProperties), nameof(VerbProperties.range));

                var adjustedRangeInfo = AccessTools.Method(typeof(manual_GoToCastPosition_initAction), nameof(AdjustedRange));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == rangeInfo)
                    {
                        yield return instruction; // curJob.verbToUse.verbProps.range
                        yield return new CodeInstruction(OpCodes.Ldloc_1); // curJob
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // pawn
                        instruction = new CodeInstruction(OpCodes.Call, adjustedRangeInfo); // AdjustedRange(curJob.verbToUse.verbProps.range, curJob, actor)
                    }

                    yield return instruction;
                }
            }

            private static float AdjustedRange(float original, Job job, Pawn actor)
            {
                return TrenchUtility.FinalAdjustedRangeFromTerrain(original, job.verbToUse, actor);
            }

        }

    }

}
