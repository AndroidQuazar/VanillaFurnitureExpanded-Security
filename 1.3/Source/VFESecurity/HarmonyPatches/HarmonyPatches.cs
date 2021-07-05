using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace VFESecurity
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            // Unlimited range
            int range = 200;
            List<IntVec3> list = new List<IntVec3>();

            for (int i = -range; i < range; i++)
            {
                for (int j = -range; j < range; j++)
                {
                    list.Add(new IntVec3(i, 0, j));
                }
            }
            list.Sort(delegate (IntVec3 A, IntVec3 B)
            {
                float num = A.LengthHorizontalSquared;
                float num2 = B.LengthHorizontalSquared;
                if (num < num2)
                {
                    return -1;
                }
                return (num != num2) ? 1 : 0;
            });

            GenRadial.RadialPattern = new IntVec3[list.Count];
            float[] radii = new float[list.Count];

            for (int k = 0; k < list.Count; k++)
            {
                GenRadial.RadialPattern[k] = list[k];
                radii[k] = list[k].LengthHorizontal;
            }
            AccessTools.Field(typeof(GenRadial), "RadialPatternRadii").SetValue(null, radii);

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
        }
    }
}