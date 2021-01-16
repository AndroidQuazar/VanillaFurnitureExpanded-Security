using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace VFESecurity
{
    [HarmonyPatch(typeof(SiegeBlueprintPlacer), "PlaceArtilleryBlueprints")]
    public static class Patch_PlaceArtilleryBlueprints
    {
        public static IEnumerable<Blueprint_Build> Postfix(IEnumerable<Blueprint_Build> __result, float points, Map map, Faction ___faction)
        {
            int numArtillery = Mathf.RoundToInt(points / 60f);
            numArtillery = Mathf.Clamp(numArtillery, 1, 2);
            List<ThingDef> possibleArtillery = DefDatabase<ThingDef>.AllDefs.Where(def => def.building != null && def.building.buildingTags.Contains("Artillery_BaseDestroyer")).ToList();
            List<ThingDef> highestTechArtillery = possibleArtillery.OrderByDescending(a => a.techLevel).GroupBy(d => d.techLevel).Select(g => g.ToList()).FirstOrDefault();
            for (int i = 0; i < numArtillery; i++)
            {
                Rot4 random = Rot4.Random;
                ThingDef thingDef = highestTechArtillery.RandomElement();
                IntVec3 intVec = (IntVec3)AccessTools.Method(typeof(SiegeBlueprintPlacer), "FindArtySpot").Invoke(null, new object[] { thingDef, random, map });
                if (!intVec.IsValid)
                {
                    yield break;
                }
                StuffCategoryDef stuffDef = thingDef.stuffCategories.RandomElement();
                var potentialStuff = DefDatabase<ThingDef>.AllDefs.Where(d => d.stuffProps?.categories.Contains(stuffDef) ?? false);
                ThingDef stuff = potentialStuff.RandomElementByWeight(d => 1 / (d.BaseMarketValue * (d.smallVolume ? 20 : 1)));
                yield return GenConstruct.PlaceBlueprintForBuild(thingDef, intVec, map, random, ___faction, stuff);
                points -= 60f;
            }
        }
    }
}
