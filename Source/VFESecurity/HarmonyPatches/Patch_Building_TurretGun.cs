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

    public static class Patch_Building_TurretGun
    {

        [HarmonyPatch(typeof(Building_TurretGun), nameof(Building_TurretGun.OrderAttack))]
        public static class OrderAttack
        {

            public static void Postfix(Building_TurretGun __instance)
            {
                if (__instance.GetComp<CompLongRangeArtillery>() is CompLongRangeArtillery artilleryComp)
                    artilleryComp.ResetForcedTarget();
            }

        }

    }

}
