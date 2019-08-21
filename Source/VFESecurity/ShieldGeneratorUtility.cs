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

    public static class ShieldGeneratorUtility
    {

        public static IEnumerable<Building_Shield> ListerShieldGenerators(this Map map, Predicate<Building_Shield> validator = null)
        {
            return map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where(b => b is Building_Shield shieldGen && (validator == null || validator(shieldGen))).Cast<Building_Shield>();
        }

    }

}
