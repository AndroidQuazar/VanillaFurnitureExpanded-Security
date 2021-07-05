using UnityEngine;
using Verse;

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