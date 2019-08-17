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
    public static class NonPublicProperties
    {

        public static Action<TurretTop, float> TurretTop_set_CurRotation = (Action<TurretTop, float>)
            Delegate.CreateDelegate(typeof(Action<TurretTop, float>), null, AccessTools.Property(typeof(TurretTop), "CurRotation").GetSetMethod(true));

    }

}
