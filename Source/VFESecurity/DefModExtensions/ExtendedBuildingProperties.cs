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
using Harmony;

namespace VFESecurity
{

    public class ExtendedBuildingProperties : DefModExtension
    {

        public static readonly ExtendedBuildingProperties defaultValues = new ExtendedBuildingProperties();

        public float shortCircuitChancePerEnergyLost;
        public float inactivePowerConsumption;
        public Color shieldColour = Color.white;

    }

}
