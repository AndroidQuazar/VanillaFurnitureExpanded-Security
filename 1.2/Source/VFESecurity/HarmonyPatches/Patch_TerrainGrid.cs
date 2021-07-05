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

    public static class Patch_TerrainGrid
    {

        [HarmonyPatch(typeof(TerrainGrid), nameof(TerrainGrid.CanRemoveTopLayerAt))]
        public static class CanRemoveTopLayerAt
        {

            public static void Postfix(Map ___map, IntVec3 c, ref bool __result)
            {
                // If anything at that point in the map has CompTerrainSetter, the terrain can't be removed
                var things = c.GetThingList(___map);
                for (int i = 0; i < things.Count; i++)
                {
                    var thing = things[i];
                    if (thing.TryGetComp<CompTerrainSetter>() != null)
                        __result = false;
                }
            }

        }


    }

}
