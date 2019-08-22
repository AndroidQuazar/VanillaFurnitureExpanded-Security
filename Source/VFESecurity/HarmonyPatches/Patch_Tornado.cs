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

    public static class Patch_Tornado
    {

        [HarmonyPatch(typeof(Tornado), "CellImmuneToDamage")]
        public static class CellImmuneToDamage
        {

            public static void Postfix(Tornado __instance, IntVec3 c, ref bool __result)
            {
                // Shield-covered cells are immune to damage
                if (!__result)
                {
                    var shieldGens = __instance.Map.GetComponent<ListerThingsExtended>().ListerShieldGensActive;
                    foreach (var gen in shieldGens)
                    {
                        if (gen.coveredCells.Contains(c))
                        {
                            if (!gen.affectedThings.ContainsKey(__instance))
                            {
                                gen.AbsorbDamage(30, DamageDefOf.TornadoScratch, __instance);
                                gen.affectedThings.Add(__instance, 15);
                            }
                            __result = true;
                            return;
                        }
                    }
                }
            }

        }

    }

}
