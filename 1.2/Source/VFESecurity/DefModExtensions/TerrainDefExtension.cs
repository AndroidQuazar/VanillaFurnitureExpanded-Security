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

    public class TerrainDefExtension : DefModExtension
    {

        private static readonly TerrainDefExtension defaultValues = new TerrainDefExtension();

        public static TerrainDefExtension Get(Def def)
        {
            return def.GetModExtension<TerrainDefExtension>() ?? defaultValues;
        }

        public bool allowCrouching;
        public int pathCostEntering = -1;
        public int pathCostLeaving = -1;
        public float coverEffectiveness;
        public float rangeFactor = 1;

    }

}
