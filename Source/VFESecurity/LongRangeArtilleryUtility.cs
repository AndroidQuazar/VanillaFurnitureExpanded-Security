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

    public static class LongRangeArtilleryUtility
    {

        public static Dictionary<ThingDef, Texture2D> launchGizmoIconCache;

        public static void SetCache()
        {
            launchGizmoIconCache = new Dictionary<ThingDef, Texture2D>();
            foreach (var tDef in DefDatabase<ThingDef>.AllDefsListForReading)
                if (tDef.GetCompProperties<CompProperties_LongRangeArtillery>() is CompProperties_LongRangeArtillery artilleryProps)
                    launchGizmoIconCache.Add(tDef, ContentFinder<Texture2D>.Get(artilleryProps.gizmoIconTexPath));
        }

    }

}
