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

    public class CompProperties_TerrainSetter : CompProperties
    {

        public CompProperties_TerrainSetter()
        {
            compClass = typeof(CompTerrainSetter);
        }

        public TerrainDef terrainDef;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (terrainDef == null)
            {
                yield return "terrainDef has not been defined. Defaulting to Concrete...";
                terrainDef = TerrainDefOf.Concrete;
            }
        }

    }

}
