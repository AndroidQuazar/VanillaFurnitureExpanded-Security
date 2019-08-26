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

    public static class Patch_SectionLayer_SunShadows
    {

        public static class manual_Regenerate
        {

            [HarmonyPriority(Priority.First)]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var staticSunShadowHeightInfo = AccessTools.Field(typeof(ThingDef), nameof(ThingDef.staticSunShadowHeight));

                var adjustedStatisSunShadowHeightInfo = AccessTools.Method(typeof(manual_Regenerate), nameof(AdjustedStatisSunShadowHeight));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == staticSunShadowHeightInfo)
                    {
                        yield return instruction; // thing.def.staticSunShadowHeight
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 4); // thing
                        instruction = new CodeInstruction(OpCodes.Call, adjustedStatisSunShadowHeightInfo); // AdjustedStatisSunShadowHeight(thing.def.staticSunShadowHeight, thing)
                    }

                    yield return instruction;
                }
            }

            private static float AdjustedStatisSunShadowHeight(float original, Thing thing)
            {
                if (thing.IsSubmersible(out CompSubmersible submersibleComp) && submersibleComp.Submerged)
                    return submersibleComp.Props.submergedStaticSunShadowHeight;
                return original;
            }

        }
        

    }

}
