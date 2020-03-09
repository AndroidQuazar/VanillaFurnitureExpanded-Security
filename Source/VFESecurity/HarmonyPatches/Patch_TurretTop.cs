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

    public static class Patch_TurretTop
    {

        [HarmonyPatch(typeof(TurretTop), nameof(TurretTop.DrawTurret))]
        public static class DrawTurret
        {

            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(Building_Turret ___parentTurret)
            {
                // Don't draw the turret top if the turret is retracted
                if (___parentTurret.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
                    return false;
                return true;
            }

        }

    }

}
