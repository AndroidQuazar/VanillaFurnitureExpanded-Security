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

    public static class Patch_Building_TurretGun
    {


        [HarmonyPatch(typeof(Building_TurretGun), nameof(Building_TurretGun.DrawExtraSelectionOverlays))]
        public static class DrawExtraSelectionOverlays
        {

            public static void Postfix(Building_TurretGun __instance)
            {
                var artilleryComp = __instance.TryGetComp<CompLongRangeArtillery>();
                if (artilleryComp != null && artilleryComp.targetedTile != TargetInfo.Invalid)
                {
                    var edgeCell = artilleryComp.FacingEdgeCell;

                    // Warmup pie
                    if (artilleryComp.CanLaunch)
                    {
                        if (artilleryComp.warmupTicksLeft > 0)
                            GenDraw.DrawAimPie(__instance, edgeCell, (int)(artilleryComp.warmupTicksLeft * 0.5f), __instance.def.size.x * 0.5f);
                    }

                    // Targeting lines
                    var a = __instance.TrueCenter();
                    var b = edgeCell.CenterVector3;
                    a.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                    b.y = a.y;
                    GenDraw.DrawLineBetween(a, b, Building_TurretGun.ForcedTargetLineMat);
                }
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
