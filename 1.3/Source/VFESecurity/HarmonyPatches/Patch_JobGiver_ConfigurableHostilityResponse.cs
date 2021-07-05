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

    public static class Patch_JobGiver_ConfigurableHostilityResponse
    {

        [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetAttackNearbyEnemyJob")]
        public static class TryGetAttackNearbyEnemyJob
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("Transpiler start: JobGiver_ConfigurableHostilityResponse.TryGetAttackNearbyEnemyJob (1 match)");
                #endif

                var instructionList = instructions.ToList();

                var rangeInfo = AccessTools.Field(typeof(VerbProperties), nameof(VerbProperties.range));

                var adjustedRangeInfo = AccessTools.Method(typeof(TryGetAttackNearbyEnemyJob), nameof(AdjustedRange));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(rangeInfo))
                    {
                        #if DEBUG
                            Log.Message("JobGiver_ConfigurableHostilityResponse.TryGetAttackNearbyEnemyJob match 1 of 1");
                        #endif

                        yield return instruction; // pawn.CurrentEffectiveVerb.verbProps.range
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // pawn
                        instruction = new CodeInstruction(OpCodes.Call, adjustedRangeInfo); // AdjustedRange(verb.verbProps.range, pawn)
                    }

                    yield return instruction;
                }
            }

            private static float AdjustedRange(float original, Pawn pawn)
            {
                return TrenchUtility.FinalAdjustedRangeFromTerrain(original, pawn.CurrentEffectiveVerb.verbProps.minRange, pawn.Position, pawn.Map);
            }

        }

    }

}
