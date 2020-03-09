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

    public static class Patch_Thing
    {

        [HarmonyPatch(typeof(Thing), nameof(Thing.BlocksPawn))]
        public static class BlocksPawn
        {

            public static void Postfix(Thing __instance, ref bool __result)
            {
                if (__instance.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
                    __result = retractableComp.Props.submergedPassability == Traversability.Impassable;
            }

        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.DeSpawn))]
        public static class DeSpawn
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // Transpiler is flexible enough to allow for reuse
                return Patch_PathGrid.CalculatedCostAt.Transpiler(instructions);
            }

        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.Graphic), MethodType.Getter)]
        public static class get_Graphic
        {

            [HarmonyPriority(Priority.Last)]
            public static void Postfix(Thing __instance, ref Graphic __result)
            {
                if (__instance.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged && retractableComp.SubmergedGraphic != null)
                    __result = retractableComp.SubmergedGraphic;
            }

        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
        public static class SpawnSetup
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // Transpiler is flexible enough to allow for reuse
                return Patch_PathGrid.CalculatedCostAt.Transpiler(instructions);
            }

        }
        

    }

}
