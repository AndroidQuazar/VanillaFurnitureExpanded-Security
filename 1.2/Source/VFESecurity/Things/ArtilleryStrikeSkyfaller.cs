using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace VFESecurity
{

    public abstract class ArtilleryStrikeSkyfaller : Skyfaller
    {

        private Sustainer ambientSustainer;

        protected abstract ThingDef ShellDef
        {
            get;
        }

        public override void Tick()
        {
            // Sounds
            if (ambientSustainer == null && !ShellDef.projectile.soundAmbient.NullOrUndefined())
                ambientSustainer = ShellDef.projectile.soundAmbient.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            if (ambientSustainer != null)
                ambientSustainer.Maintain();

            base.Tick();
        }

    }

}
