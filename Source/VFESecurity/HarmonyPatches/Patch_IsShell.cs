using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFESecurity
{
    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsShell), MethodType.Getter)]
    public static class Patch_IsShell
    {
        public static void Postfix(ref bool __result, ThingDef __instance)
        {
            __result = __result && !__instance.IsWithinCategory(ThingCategoryDefOf.StoneChunks);
        }
    }
}
