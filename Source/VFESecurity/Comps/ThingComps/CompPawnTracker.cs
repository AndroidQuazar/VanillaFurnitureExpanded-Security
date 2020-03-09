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

    public class CompPawnTracker : ThingComp
    {

        private const int HighPylonExposureThreshold = 1800;

        private Pawn Pawn => (Pawn)parent;

        public override void CompTick()
        {
            if (psychicPylonExposureTicks > 0)
                psychicPylonExposureTicks--;
        }

        public bool HighPsychicPylonExposure
        {
            get
            {
                float psychicSensitivity = Pawn.GetStatValue(RimWorld.StatDefOf.PsychicSensitivity);
                if (psychicSensitivity > 0)
                {
                    int finalExposureTicks = Mathf.RoundToInt(HighPylonExposureThreshold / psychicSensitivity);
                    return psychicPylonExposureTicks >= finalExposureTicks;
                }
                return false;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref psychicPylonExposureTicks, "psychicPylonExposureTicks");
        }

        public int psychicPylonExposureTicks;

    }

}
