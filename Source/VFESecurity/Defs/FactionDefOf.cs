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
    public static class FactionDefNamed
    {

        public static readonly FactionDef VFEI_Insect = DefDatabase<FactionDef>.GetNamedSilentFail("VFEI_Insect");

    }

}
