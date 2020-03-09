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

    public static class SubmersibleUtility
    {

        public static bool IsSubmersible(this Thing thing, out CompSubmersible submersibleComp)
        {
            submersibleComp = thing.TryGetComp<CompSubmersible>();
            return submersibleComp != null;
        }

    }

}
