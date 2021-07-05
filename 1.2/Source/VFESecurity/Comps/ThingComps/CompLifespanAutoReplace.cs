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
using HarmonyLib;

namespace VFESecurity
{

    public class CompLifespanAutoReplace : CompLifespan
    {

        public override void CompTick()
        {
            bool spawned = parent.Spawned;
            var map = parent.Map;
            base.CompTick();
            if (spawned)
                ThingUtility.CheckAutoRebuildOnDestroyed(parent, DestroyMode.KillFinalize, map, parent.def);
        }

        public override void CompTickRare()
        {
            bool spawned = parent.Spawned;
            var map = parent.Map;
            base.CompTickRare();
            if (spawned)
                ThingUtility.CheckAutoRebuildOnDestroyed(parent, DestroyMode.KillFinalize, map, parent.def);
        }

    }

}
