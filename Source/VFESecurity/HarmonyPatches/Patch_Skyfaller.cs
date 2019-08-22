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

    public static class Patch_Skyfaller
    {

        [HarmonyPatch(typeof(Skyfaller), nameof(Skyfaller.SpawnSetup))]
        public static class SpawnSetup
        {

            public static void Postfix(Skyfaller __instance, Map map)
            {
                // Check for shield intercepts
                var thingDefExtension = __instance.def.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues;
                ShieldGeneratorUtility.CheckIntercept(__instance, map, thingDefExtension.shieldDamageIntercepted, DamageDefOf.Blunt, () => __instance.OccupiedRect().Cells, () => thingDefExtension.shieldDamageIntercepted > -1,
                    postIntercept: s => 
                    {
                        if (s.Energy > 0)
                            __instance.Destroy();
                    });
            }

        }

    }

}
