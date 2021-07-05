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

    public static class TrenchUtility
    {

        public static float FinalAdjustedRangeFromTerrain(float range, Verb verb)
        {
            return FinalAdjustedRangeFromTerrain(range, verb.verbProps.minRange, verb.caster.Position, verb.caster.Map);
        }

        public static float FinalAdjustedRangeFromTerrain(float range, Verb verb, Thing thing)
        {
            return FinalAdjustedRangeFromTerrain(range, verb.verbProps.minRange, thing.Position, thing.Map);
        }

        public static float FinalAdjustedRangeFromTerrain(float range, float minRange, LocalTargetInfo target)
        {
            return FinalAdjustedRangeFromTerrain(range, minRange, target, Find.CurrentMap);
        }

        public static float FinalAdjustedRangeFromTerrain(float range, float minRange, IntVec3 pos)
        {
            return FinalAdjustedRangeFromTerrain(range, minRange, pos, Find.CurrentMap);
        }

        public static float FinalAdjustedRangeFromTerrain(float range, float minRange, LocalTargetInfo target, Map map)
        {
            return Mathf.Max(AdjustedRangeFromTerrain(range, target, map), minRange + 1);
        }

        public static float FinalAdjustedRangeFromTerrain(float range, float minRange, IntVec3 pos, Map map)
        {
            return Mathf.Max(AdjustedRangeFromTerrain(range, pos, map), minRange + 1);
        }

        private static float AdjustedRangeFromTerrain(float range, LocalTargetInfo target, Map map)
        {
            return AdjustedRangeFromTerrain(range, target.Cell, map);
        }

        private static float AdjustedRangeFromTerrain(float range, IntVec3 pos, Map map)
        {
            float original = range;
            var terrain = map.terrainGrid.TerrainAt(pos);
            var terrainDefExtension = TerrainDefExtension.Get(terrain);
            if (terrainDefExtension.rangeFactor != 1)
                range *= terrainDefExtension.rangeFactor;
            return original - Mathf.RoundToInt(original - range);
        }

    }

}
