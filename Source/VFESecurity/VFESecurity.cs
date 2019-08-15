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

    public class VFESecurity : Mod
    {

        public VFESecurity(ModContentPack content) : base(content)
        {
            HarmonyInstance = HarmonyInstance.Create("OskarPotocki.VanillaFurnitureExpanded.Security");
        }

        public static HarmonyInstance HarmonyInstance;

    }

}
