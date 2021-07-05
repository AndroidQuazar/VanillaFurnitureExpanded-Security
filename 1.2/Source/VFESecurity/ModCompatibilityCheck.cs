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
    public static class ModCompatibilityCheck
    {

        static ModCompatibilityCheck()
        {
            var activeModList = ModsConfig.ActiveModsInLoadOrder.ToList();
            for (int i = 0; i < activeModList.Count; i++)
            {
                var mod = activeModList[i];
                if (mod.Name == "Vanilla Factions Expanded - Insectoids")
                    VanillaFactionsExpandedInsectoids = true;
            }
        }

        public static bool VanillaFactionsExpandedInsectoids;

    }

}
