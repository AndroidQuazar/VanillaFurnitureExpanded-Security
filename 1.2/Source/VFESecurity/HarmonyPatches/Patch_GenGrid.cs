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

    public static class Patch_GenGrid
    {

        [HarmonyPatch(typeof(GenGrid), nameof(GenGrid.Standable))]
        public static class Standable
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("Transpiler start: GenGrid.Standable (1 match)");
                #endif

                // Decided to do as a transpiler for performance reasons
                var instructionList = instructions.ToList();

                var getItemInfo = AccessTools.Method(typeof(List<Thing>), "get_Item");

                var passabilityInfo = AccessTools.Field(typeof(BuildableDef), nameof(BuildableDef.passability));

                var actualPassabilityInfo = AccessTools.Method(typeof(Standable), nameof(ActualPassability));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(passabilityInfo))
                    {
                        #if DEBUG
                            Log.Message("GenGrid.Standable match 1 of 1");
                        #endif

                        yield return instruction; // list[i].def.passability
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // list
                        yield return new CodeInstruction(OpCodes.Ldloc_1); // i
                        yield return new CodeInstruction(OpCodes.Callvirt, getItemInfo); // list[i]
                        instruction = new CodeInstruction(OpCodes.Call, actualPassabilityInfo); // ActualPassability(list[i].def.passability, list[i])
                    }

                    yield return instruction;
                }
            }

            private static Traversability ActualPassability(Traversability original, Thing thing)
            {
                if (thing.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
                    return retractableComp.Props.submergedPassability;
                return original;
            }

        }

        [HarmonyPatch(typeof(GenGrid), nameof(GenGrid.Impassable))]
        public static class Impassable
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // Methods are similar enough that the same transpiler can be used
                return Standable.Transpiler(instructions);
            }

        }

    }

}
