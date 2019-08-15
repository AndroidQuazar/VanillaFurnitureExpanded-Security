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

    public static class Patch_Thing
    {

        [HarmonyPatch(typeof(Thing), nameof(Thing.BlocksPawn))]
        public static class BlocksPawn
        {

            public static void Postfix(Thing __instance, ref bool __result)
            {
                if (__instance.IsRetractable(out CompRetractable retractableComp) && retractableComp.Retracted)
                    __result = retractableComp.Props.retractedPassability == Traversability.Impassable;
            }

        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.Graphic), MethodType.Getter)]
        public static class get_Graphic
        {

            [HarmonyPriority(Priority.Last)]
            public static void Postfix(Thing __instance, ref Graphic __result)
            {
                if (__instance.IsRetractable(out CompRetractable retractableComp) && retractableComp.Retracted && retractableComp.RetractedGraphic != null)
                    __result = retractableComp.RetractedGraphic;
            }

        }

    }

}
