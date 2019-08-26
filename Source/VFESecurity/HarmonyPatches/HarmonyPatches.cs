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

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            VFESecurity.HarmonyInstance.PatchAll();

            // Anonymous types...
            var bestAttackTargetAnon = typeof(AttackTargetFinder).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).
                Where(t => t.Name.Contains("BestAttackTarget")).MaxBy(t => t.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Count());
            Patch_AttackTargetFinder.manual_BestAttackTarget.firstAnonymousType = bestAttackTargetAnon;
            VFESecurity.HarmonyInstance.Patch(AccessTools.Method(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestAttackTarget)), transpiler: new HarmonyMethod(typeof(Patch_AttackTargetFinder.manual_BestAttackTarget), "Transpiler"));

            // Patch the InitAction for Toils_Combat.GoToCastPosition
            var initActionType = typeof(Toils_Combat).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.Name.Contains("GotoCastPosition"));
            VFESecurity.HarmonyInstance.Patch(initActionType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(), transpiler: new HarmonyMethod(typeof(Patch_Toils_Combat.manual_GoToCastPosition_initAction), "Transpiler"));

            // Why, oh why does this class have to be internal?
            var sectionLayerSunShadows = GenTypes.GetTypeInAnyAssemblyNew("Verse.SectionLayer_SunShadows", "Verse");
            VFESecurity.HarmonyInstance.Patch(AccessTools.Method(sectionLayerSunShadows, "Regenerate"), transpiler: new HarmonyMethod(typeof(Patch_SectionLayer_SunShadows.manual_Regenerate), "Transpiler"));
        }

    }

}
