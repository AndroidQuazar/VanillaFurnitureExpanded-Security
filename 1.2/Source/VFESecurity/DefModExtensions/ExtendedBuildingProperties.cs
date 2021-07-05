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

    public class ExtendedBuildingProperties : DefModExtension
    {

        public static readonly ExtendedBuildingProperties defaultValues = new ExtendedBuildingProperties();

        // Shield generators
        public float initialEnergyPercentage;
        public int rechargeTicksWhenDepleted;
        public float shortCircuitChancePerEnergyLost;
        public float inactivePowerConsumption;
        public Color shieldColour = Color.white;

        // Traps
        public float trapDestroyOnSpringChance;

    }

}
