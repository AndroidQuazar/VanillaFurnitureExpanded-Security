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

    public static class Patch_CoverUtility
    {

        [HarmonyPatch(typeof(CoverUtility), nameof(CoverUtility.BaseBlockChance), new Type[] { typeof(Thing) })]
        public static class BaseBlockChance_Thing
        {

            public static void Postfix(Thing thing, ref float __result)
            {
                // Modify base block chance of retracted things
                if (__result > 0 && thing.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
                    __result *= retractableComp.Props.submergedProjectileBlockChance / __result;
            }

        }

        [HarmonyPatch(typeof(CoverUtility), nameof(CoverUtility.CalculateCoverGiverSet))]
        public static class CalculateCoverGiverSet
        {

            public static void Postfix(LocalTargetInfo target, Map map, ref List<CoverInfo> __result)
            {
                var things = target.Cell.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    var thing = things[i];
                    var terrainSetter = thing.def.GetCompProperties<CompProperties_TerrainSetter>();
                    if (terrainSetter != null)
                    {
                        var terrain = terrainSetter.terrainDef;
                        var terrainDefExtension = TerrainDefExtension.Get(terrain);
                        if (terrainDefExtension.coverEffectiveness > 0)
                        {
                            __result.Add(new CoverInfo(thing, terrainDefExtension.coverEffectiveness));
                        }
                    }
                }
            }

        }

        [HarmonyPatch(typeof(CoverUtility), nameof(CoverUtility.CalculateOverallBlockChance))]
        public static class CalculateOverallBlockChance
        {

            public static void Postfix(LocalTargetInfo target, Map map, ref float __result)
            {
                var things = target.Cell.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    var thing = things[i];
                    var terrainSetter = thing.def.GetCompProperties<CompProperties_TerrainSetter>();
                    if (terrainSetter != null)
                    {
                        var terrain = terrainSetter.terrainDef;
                        var terrainDefExtension = TerrainDefExtension.Get(terrain);
                        if (terrainDefExtension.coverEffectiveness > 0)
                        {
                            __result += (1 - __result) * terrainDefExtension.coverEffectiveness;
                        }
                    }
                }
            }

        }

    }

}
