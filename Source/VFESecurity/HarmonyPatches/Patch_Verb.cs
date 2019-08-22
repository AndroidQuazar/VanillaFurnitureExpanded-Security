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
        

    }

}
