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

    public static class Patch_CastPositionFinder
    {

        [HarmonyPatch(typeof(CastPositionFinder), nameof(CastPositionFinder.TryFindCastPosition))]
        public static class TryFindCastPosition
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var rangeInfo = AccessTools.Field(typeof(VerbProperties), nameof(VerbProperties.range));

                var verbInfo = AccessTools.Field(typeof(CastPositionFinder), "verb");

                var adjustedRangeInfo = AccessTools.Method(typeof(TrenchUtility), nameof(TrenchUtility.FinalAdjustedRangeFromTerrain), new Type[] { typeof(float), typeof(Verb)});

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == rangeInfo)
                    {
                        yield return instruction; // CastPositionFinder.verb.verbProps.range
                        yield return new CodeInstruction(OpCodes.Ldsfld, verbInfo); // CastPositionFinder.verb
                        instruction = new CodeInstruction(OpCodes.Call, adjustedRangeInfo); // AdjustedRange(CastPositionFinder.verb.verbProps.range, CastPositionFinder.verb)
                    }

                    yield return instruction;
                }
            }

        }

    }

}
