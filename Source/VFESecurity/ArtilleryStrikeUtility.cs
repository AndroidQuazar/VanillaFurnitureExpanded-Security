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
using Harmony;

namespace VFESecurity
{

    public static class ArtilleryStrikeUtility
    {

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
            var artilleryStrikeIncoming = (ArtilleryStrikeIncoming)SkyfallerMaker.MakeSkyfaller(ThingDefOf.VFE_ArtilleryStrikeIncoming);
            artilleryStrikeIncoming.artilleryShellDef = shellDef;
            return (ArtilleryStrikeIncoming)GenSpawn.Spawn(artilleryStrikeIncoming, position, map);
        }

    }

}
