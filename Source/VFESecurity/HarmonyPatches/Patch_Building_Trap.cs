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

    public static class Patch_Building_Trap
    {


        [HarmonyPatch(typeof(Building_Trap), nameof(Building_Trap.Spring))]
        public static class Spring
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("Transpiler start: Building_Trap.Spring (1 match)");
                #endif

                var instructionList = instructions.ToList();

                var trapDestroyOnSpringInfo = AccessTools.Field(typeof(BuildingProperties), nameof(BuildingProperties.trapDestroyOnSpring));

                var shouldDestroyInfo = AccessTools.Method(typeof(Spring), nameof(ShouldDestroy));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(trapDestroyOnSpringInfo))
                    {
                        #if DEBUG
                            Log.Message("Building_Trap.Spring match 1 of 1");
                        #endif

                        yield return instruction;  // this.def.building.trapDestroyOnSpring
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        instruction = new CodeInstruction(OpCodes.Call, shouldDestroyInfo); // ShouldDestroy(this.def.building.trapDestroyOnSpring, this)
                    }

                    yield return instruction;
                }
            }

            private static bool ShouldDestroy(bool original, Building_Trap instance)
            {
                if (!original)
                {
                    var extBuldingProps = instance.def.GetModExtension<ExtendedBuildingProperties>() ?? ExtendedBuildingProperties.defaultValues;
                    if (Rand.Chance(extBuldingProps.trapDestroyOnSpringChance))
                        return true;
                }
                return original;
            }

        }

    }

}
