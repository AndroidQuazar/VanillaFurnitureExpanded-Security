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

    public static class NonPublicTypes
    {

        [StaticConstructorOnStartup]
        public static class CombatExtended
        {

            static CombatExtended()
            {
                if (ModCompatibilityCheck.CombatExtended)
                {
                    ProjectileCE = GenTypes.GetTypeInAnyAssemblyNew("CombatExtended.ProjectileCE", "CombatExtended");
                }
            }

            public static Type ProjectileCE;

        }

    }

}
