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

    public static class Patch_AttackTargetFinder
    {

        // [HarmonyPatch(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestAttackTarget))]
        public static class manual_BestAttackTarget
        {

            public static Type firstAnonymousType;

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var rangeInfo = AccessTools.Field(typeof(VerbProperties), nameof(VerbProperties.range));

                var verbInfo = AccessTools.Field(firstAnonymousType, "verb");
                var searcherThingInfo = AccessTools.Field(firstAnonymousType, "searcherThing");

                var adjustedRangeInfo = AccessTools.Method(typeof(TrenchUtility), nameof(TrenchUtility.FinalAdjustedRangeFromTerrain), new Type[] { typeof(float), typeof(Verb), typeof(Thing) });

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == rangeInfo)
                    {
                        yield return instruction; // verb.verbProps.range
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // anon1
                        yield return new CodeInstruction(OpCodes.Ldfld, verbInfo); // anon1.verb
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // anon1
                        yield return new CodeInstruction(OpCodes.Ldfld, searcherThingInfo); // anon1.searcherThing
                        instruction = new CodeInstruction(OpCodes.Call, adjustedRangeInfo); // AdjustedRange(verb.verbProps.range, anon1.verb, anon1.searcherThing)
                    }

                    yield return instruction;
                }
            }

        }

        [HarmonyPatch(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestShootTargetFromCurrentPosition))]
        public static class BestShootTargetFromCurrentPosition
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var rangeInfo = AccessTools.Field(typeof(VerbProperties), nameof(VerbProperties.range));

                var adjustedRangeInfo = AccessTools.Method(typeof(BestShootTargetFromCurrentPosition), nameof(AdjustedRange));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == rangeInfo)
                    {
                        yield return instruction; // verb.verbProps.range
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // currentEffectiveVerb
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // searcher
                        instruction = new CodeInstruction(OpCodes.Call, adjustedRangeInfo); // AdjustedRange(verb.verbProps.range, currentEffectiveVerb, searcher)
                    }

                    yield return instruction;
                }
            }

            private static float AdjustedRange(float original, Verb verb, IAttackTargetSearcher searcher)
            {
                return TrenchUtility.FinalAdjustedRangeFromTerrain(original, verb, searcher.Thing);
            }

        }

    }

}
