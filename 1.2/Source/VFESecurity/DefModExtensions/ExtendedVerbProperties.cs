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
using RimWorld.Planet;
using HarmonyLib;

namespace VFESecurity
{

    public class ExtendedVerbProperties : DefModExtension
    {

        public static readonly ExtendedVerbProperties defaultValues = new ExtendedVerbProperties();

        public float illuminatedRadius;
        public int dazzleDurationTicks;

    }

}
