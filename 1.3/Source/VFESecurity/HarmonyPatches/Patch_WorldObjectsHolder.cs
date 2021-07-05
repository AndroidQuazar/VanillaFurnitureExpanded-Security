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

    public static class Patch_WorldObjectsHolder
    {

        [HarmonyPatch(typeof(WorldObjectsHolder), nameof(WorldObjectsHolder.Remove))]
        public static class Remove
        {

            public static void Postfix(WorldObject o)
            {
                Find.World.GetComponent<WorldArtilleryTracker>().Notify_WorldObjectRemoved(o);
            }

        }

    }

}
