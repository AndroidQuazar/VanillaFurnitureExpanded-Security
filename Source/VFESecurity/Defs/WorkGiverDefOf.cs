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

    [DefOf]
    public static class WorkGiverDefOf
    {

        public static WorkGiverDef VFES_ConstructRearmTrap;
        public static WorkGiverDef VFES_RearmTrap;

    }

}
