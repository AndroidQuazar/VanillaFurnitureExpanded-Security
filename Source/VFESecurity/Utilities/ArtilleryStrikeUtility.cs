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

    public static class ArtilleryStrikeUtility
    {

        private static List<ThingDef> allowedEnemyShellDefs;

        public static void SetCache()
        {
            allowedEnemyShellDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.IsShell && t.projectileWhenLoaded.projectile.damageDef.harmsHealth).ToList();
        }

        public static ThingDef GetRandomShellFor(ThingDef artilleryGunDef, FactionDef faction)
        {
            return allowedEnemyShellDefs.Where(s => s.techLevel <= faction.techLevel && artilleryGunDef.building.defaultStorageSettings.AllowedToAccept(s)).Select(s => s.projectileWhenLoaded).RandomElement();
        }

        public static float FinalisedMissRadius(float forcedMissRadius, float maxRadiusFactor, int tileA, int tileB, int range)
        {
            return forcedMissRadius * Mathf.Lerp(1, maxRadiusFactor, (float)Find.WorldGrid.TraversalDistanceBetween(tileA, tileB) / range);
        }

        public static IEnumerable<IntVec3> PotentialStrikeCells(Map map, float missRadius)
        {
            return missRadius < GenRadial.MaxRadialPatternRadius ? GenRadial.RadialCellsAround(map.AllCells.RandomElement(), missRadius, true).Where(c => c.InBounds(map)) : map.AllCells;
        }

        public static ArtilleryStrikeIncoming SpawnArtilleryStrikeSkyfaller(ThingDef shellDef, Map map, IntVec3 position)
        {
            var artilleryStrikeIncoming = (ArtilleryStrikeIncoming)SkyfallerMaker.MakeSkyfaller(ThingDefOf.VFES_ArtilleryStrikeIncoming);
            artilleryStrikeIncoming.artilleryShellDef = shellDef;
            return (ArtilleryStrikeIncoming)GenSpawn.Spawn(artilleryStrikeIncoming, position, map);
        }

        public static IEnumerable<Vector3> WorldLineDrawPoints(Vector3 start, Vector3 end)
        {
            float dist = Vector3.Distance(start, end);
            float distDone = 0;

            while (distDone < dist)
            {
                var point = Vector3.Slerp(start, end, distDone / dist);
                point += point.normalized * 0.05f;
                yield return point;
                distDone = Mathf.Min(distDone + 2, dist);
            }

            yield return end + end.normalized * 0.05f;
        }

    }

}
