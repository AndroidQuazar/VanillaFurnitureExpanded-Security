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

    public class VFESecurity : Mod
    {

        public VFESecurity(ModContentPack content) : base(content)
        {
            #if DEBUG
                Log.Error("Somebody left debugging enabled in Vanilla Furniture Expanded Security - please let the team know!");
            #endif

            harmonyInstance = new Harmony("OskarPotocki.VanillaFurnitureExpanded.Security");
        }

        public static Harmony harmonyInstance;

    }

}
