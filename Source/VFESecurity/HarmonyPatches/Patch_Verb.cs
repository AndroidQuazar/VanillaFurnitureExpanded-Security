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

    public static class Patch_Verb
    {

        [HarmonyPatch(typeof(Verb), nameof(Verb.Available))]
        public static class Available
        {

            public static void Postfix(Verb __instance, ref bool __result)
            {
                // Submerged things can't shoot
                if (__result && __instance.caster != null && __instance.caster.IsSubmersible(out CompSubmersible submersibleComp) && submersibleComp.Submerged)
                    __result = false;
            }

        }

        [HarmonyPatch(typeof(Verb), nameof(Verb.TryFindShootLineFromTo))]
        public static class TryFindShootLineFromTo
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var rangeInfo = AccessTools.Field(typeof(VerbProperties), nameof(VerbProperties.range));

                var finaliseAdjustedRangeInfo = AccessTools.Method(typeof(TryFindShootLineFromTo), nameof(FinaliseAdjustedRange));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == rangeInfo)
                    {
                        yield return instruction; // this.verbProps.range
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        yield return new CodeInstruction(OpCodes.Ldarg_2); // targ

                        instruction = new CodeInstruction(OpCodes.Call, finaliseAdjustedRangeInfo); // FinaliseAdjustedRange(this.verbProps.range, this, targ)
                    }

                    yield return instruction;
                }
            }

            private static float FinaliseAdjustedRange(float original, Verb instance, LocalTargetInfo targ)
            {
                return TrenchUtility.FinalAdjustedRangeFromTerrain(original, instance.verbProps.minRange, targ, instance.caster.Map);
            }

        }


    }

}
