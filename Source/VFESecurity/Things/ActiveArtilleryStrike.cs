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

    public class ActiveArtilleryStrike : Thing
    {

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref artilleryShellDefs, "artilleryShellDefs", LookMode.Def, new object[0]);
            base.ExposeData();
        }

        public override void Tick()
        {
        }

        public List<ThingDef> artilleryShellDefs;

        public float Speed => artilleryShellDefs.Select(p => p.projectile.speed).Average();

    }

}
