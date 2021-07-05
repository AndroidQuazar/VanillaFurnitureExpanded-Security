using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

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
                    List<Building_Shield> shieldGens = __instance.Map.GetComponent<ListerThingsExtended>().ListerShieldGensActive.ToList();
                    for (int i = 0; i < shieldGens.Count; i++)
                    {
                        Building_Shield gen = shieldGens[i];
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