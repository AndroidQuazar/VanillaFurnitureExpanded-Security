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

    public static class Patch_ReverseDesignatorDatabase
    {

        [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
        public static class InitDesignators
        {

            public static void Postfix(ref List<Designator> ___desList)
            {
                ___desList.Add(new Designator_RearmTrap());
            }

        }

    }

}
