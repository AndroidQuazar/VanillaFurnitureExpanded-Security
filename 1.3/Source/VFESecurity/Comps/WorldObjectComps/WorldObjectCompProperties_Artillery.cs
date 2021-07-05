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

    public class WorldObjectCompProperties_Artillery : WorldObjectCompProperties
    {

        public WorldObjectCompProperties_Artillery()
        {
            compClass = typeof(ArtilleryComp);
        }

        public IntRange bombardmentDurationRange;
        public IntRange bombardmentCooldownRange;
#pragma warning disable CS0649
        private ThingDef defaultArtilleryDef;
        private Dictionary<FactionDef, ThingDef> factionArtilleryDefs;
        private SimpleCurve defaultArtilleryCountCurve;
        private Dictionary<FactionDef, SimpleCurve> factionArtilleryCountCurves;
#pragma warning restore CS0649

        public ThingDef ArtilleryDefFor(FactionDef faction)
        {
            if (factionArtilleryDefs != null && factionArtilleryDefs.TryGetValue(faction, out ThingDef artilleryDef))
                return artilleryDef;
            else if (faction.techLevel >= defaultArtilleryDef.techLevel)
                return defaultArtilleryDef;
            return null;
        }

        public int ArtilleryCountFor(FactionDef faction)
        {
            SimpleCurve countCurve;
            if (factionArtilleryCountCurves != null && factionArtilleryCountCurves.ContainsKey(faction))
                countCurve = factionArtilleryCountCurves[faction];
            else
                countCurve = defaultArtilleryCountCurve;

            return Mathf.RoundToInt(countCurve.Evaluate(Rand.Value));
        }

    }

}
