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

    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {

        public static FieldInfo Building_TurretGun_burstCooldownTicksLeft = AccessTools.Field(typeof(Building_TurretGun), "burstCooldownTicksLeft");
        public static FieldInfo Building_TurretGun_top = AccessTools.Field(typeof(Building_TurretGun), "top");

        public static FieldInfo TurretTop_ticksUntilIdleTurn = AccessTools.Field(typeof(TurretTop), "ticksUntilIdleTurn");

    }

}
