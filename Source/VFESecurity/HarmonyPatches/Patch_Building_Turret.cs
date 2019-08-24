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

    public static class Patch_Building_Turret
    {


        [HarmonyPatch(typeof(Building_Turret), nameof(Building_Turret.PreApplyDamage))]
        public static class PreApplyDamage
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var actuallyAffectedByEMPInfo = AccessTools.Method(typeof(PreApplyDamage), nameof(ActuallyAffectedByEMP));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldc_I4_1)
                    {
                        yield return instruction; // true
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        instruction = new CodeInstruction(OpCodes.Call, actuallyAffectedByEMPInfo); // ActuallyAffectedByEMP(true, this)
                    }

                    yield return instruction;
                }
            }

            private static bool ActuallyAffectedByEMP(bool original, Building_Turret instance)
            {
                // Horray for vanilla bugfixes!
                if (original)
                {
                    var thingDefExtension = instance.def.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues;
                    if (thingDefExtension.affectedByEMPs.HasValue)
                        return thingDefExtension.affectedByEMPs.Value;
                    return instance.GetComp<CompPower>() != null;
                }
                return original;
            }

        }

        [HarmonyPatch(typeof(Building_TurretGun), nameof(Building_TurretGun.OrderAttack))]
        public static class OrderAttack
        {

            public static void Postfix(Building_TurretGun __instance)
            {
                if (__instance.GetComp<CompLongRangeArtillery>() is CompLongRangeArtillery artilleryComp)
                    artilleryComp.ResetForcedTarget();
            }

        }

        [HarmonyPatch(typeof(Building_TurretGun), "TryStartShootSomething")]
        public static class TryStartShootSomething
        {

            public static bool Prefix(Building_TurretGun __instance)
            {
                // Don't try and automatically target if targeting a world tile
                var artilleryComp = __instance.GetComp<CompLongRangeArtillery>();
                if (artilleryComp != null && artilleryComp.targetedTile.IsValid)
                    return false;
                return true;
            }

        }

    }

}
