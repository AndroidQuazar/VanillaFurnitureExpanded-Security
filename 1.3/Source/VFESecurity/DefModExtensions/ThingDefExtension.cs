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

    public class ThingDefExtension : DefModExtension
    {

        public static readonly ThingDefExtension defaultValues = new ThingDefExtension();

        // Mortar shell projectile defs
        public GraphicData incomingSkyfallerGraphicData;

        // Skyfaller defs
        public int shieldDamageIntercepted = -1;

        // Turrets
        public bool? affectedByEMPs;

    }

}
