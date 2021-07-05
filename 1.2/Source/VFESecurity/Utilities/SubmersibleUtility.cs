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
            submersibleComp = null;
            if (thing is Submersible || thing is SubmersibleBuilding_TurretGun)
            {
                submersibleComp = thing.TryGetComp<CompSubmersible>();
                if (submersibleComp is null)
                {
                    Log.Warning($"Tried to get non-existant CompSubmersible for {thing} when thing uses Submersible thingClass. This will cause performance issues.");
                }
            }
            return submersibleComp != null;
        }

    }

}
