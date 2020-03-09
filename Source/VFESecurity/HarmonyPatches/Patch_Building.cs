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

    public static class Patch_Building
    {


        [HarmonyPatch(typeof(Building), nameof(Building.PreApplyDamage))]
        public static class PreApplyDamage
        {

            public static void Postfix(Building __instance, ref DamageInfo dinfo)
            {
                // Modify the damage taken by submerged buildings
                if (__instance.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
                {
                    dinfo.SetAmount(dinfo.Amount * retractableComp.Props.submergedDamageFactor);
                }
            }

        }

    }

}
