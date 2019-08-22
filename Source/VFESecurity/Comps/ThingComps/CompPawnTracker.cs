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

    public class CompPawnTracker : ThingComp
    {

        private const int BaseExposureForExtremeDrone = 1800;

        private Pawn Pawn => (Pawn)parent;

        public override void CompTick()
        {
            if (dazzledTicks > 0)
                dazzledTicks--;

            if (psychicPylonExposureTicks > 0)
                psychicPylonExposureTicks--;
        }

        public int PsychicPylonThoughtDegree
        {
            get
            {
                int finalExposureTicks = Mathf.RoundToInt(BaseExposureForExtremeDrone * Pawn.GetStatValue(RimWorld.StatDefOf.PsychicSensitivity));
                return psychicPylonExposureTicks < finalExposureTicks ? 3 : 4;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref dazzledTicks, "dazzledTicks");
            Scribe_Values.Look(ref psychicPylonExposureTicks, "psychicPylonExposureTicks");
        }

        public int dazzledTicks;
        public int psychicPylonExposureTicks;

    }

}
