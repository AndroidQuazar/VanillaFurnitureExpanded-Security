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

    public static class Patch_CoverUtility
    {

        [HarmonyPatch(typeof(CoverUtility), nameof(CoverUtility.BaseBlockChance), new Type[] { typeof(Thing) })]
        public static class BaseBlockChance_Thing
        {

            public static void Postfix(Thing thing, ref float __result)
            {
                // Modify base block chance of retracted things
                if (__result > 0 && thing.IsRetractable(out CompRetractable retractableComp) && retractableComp.Retracted)
                    __result *= retractableComp.Props.retractedProjectileBlockChance / __result;
            }

        }

    }

}
