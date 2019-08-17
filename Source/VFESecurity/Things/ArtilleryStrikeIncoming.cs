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

    public class ArtilleryStrikeIncoming : Skyfaller
    {

        public ThingDef artilleryShellDef;

        protected override void Impact()
        {
            var projectile = (Projectile)ThingMaker.MakeThing(artilleryShellDef);
            GenSpawn.Spawn(projectile, Position, Map);
            NonPublicMethods.Projectile_ImpactSomething(projectile);
            base.Impact();
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref artilleryShellDef, "artilleryShellDef");
            base.ExposeData();
        }

    }

}
