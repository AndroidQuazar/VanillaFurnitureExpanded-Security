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

    [StaticConstructorOnStartup]
    public class CompProperties_LongRangeArtillery : CompProperties
    {

        public CompProperties_LongRangeArtillery()
        {
            compClass = typeof(CompLongRangeArtillery);
        }

        public int worldTileRange;
        public float maxForcedMissRadiusFactor;
        public string gizmoIconTexPath;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (!parentDef.IsBuildingArtificial || !parentDef.building.IsTurret)
                yield return "parentDef is not a turret.";
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (var entry in base.SpecialDisplayStats(req))
                yield return entry;

            // World tile range
            yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "VFESecurity.WorldTileRange".Translate(), worldTileRange.ToString(), String.Empty, 0);
        }

    }

}
