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

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            #if DEBUG
                Harmony.DEBUG = true;
            #endif

            VFESecurity.harmonyInstance.PatchAll();

            // Anonymous types...
            var bestAttackTargetAnon = typeof(AttackTargetFinder).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).
                Where(t => t.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Any(m => m.Name.Contains("BestAttackTarget"))).
                MaxBy(t => t.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Count());
            Patch_AttackTargetFinder.manual_BestAttackTarget.firstAnonymousType = bestAttackTargetAnon;
            VFESecurity.harmonyInstance.Patch(AccessTools.Method(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestAttackTarget)), transpiler: new HarmonyMethod(typeof(Patch_AttackTargetFinder.manual_BestAttackTarget), "Transpiler"));

            // Patch the InitAction for Toils_Combat.GoToCastPosition
            var initActionType = typeof(Toils_Combat).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).
                First(t => t.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Any(m => m.Name.Contains("GotoCastPosition")));
            VFESecurity.harmonyInstance.Patch(initActionType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(), transpiler: new HarmonyMethod(typeof(Patch_Toils_Combat.manual_GoToCastPosition_initAction), "Transpiler"));

            // Why, oh why does this class have to be internal?
            var sectionLayerSunShadows = GenTypes.GetTypeInAnyAssembly("Verse.SectionLayer_SunShadows", "Verse");
            VFESecurity.harmonyInstance.Patch(AccessTools.Method(sectionLayerSunShadows, "Regenerate"), transpiler: new HarmonyMethod(typeof(Patch_SectionLayer_SunShadows.manual_Regenerate), "Transpiler"));

            //VFESecurity.harmonyInstance.Patch(AccessTools.Method(typeof(JobGiver_Work), nameof(JobGiver_Work.TryIssueJobPackage)),
            //    prefix: new HarmonyMethod(typeof(HarmonyPatches),
            //    nameof(TestPrefix)),
            //    postfix: new HarmonyMethod(typeof(HarmonyPatches),
            //    nameof(TestPostfix)));
        }

        public static void TestPrefix()
        {

        }

        public static void TestPostfix(Pawn pawn, JobIssueParams jobParams, ref ThinkResult __result)
        {
        }
    }

}
