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

    public class CompThingTracker : ThingComp
    {

        public override void CompTick()
        {
            if (dazzledTicks > 0)
                dazzledTicks--;
            if (illuminatedTicks > 0)
                dazzledTicks--;
        }

        public bool Illuminated
        {
            get => illuminatedTicks > 0;
            set => illuminatedTicks = 2;
        }

        public bool Dazzled => dazzledTicks > 0;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref dazzledTicks, "dazzledTicks");
            Scribe_Values.Look(ref illuminatedTicks, "illuminatedTicks");
        }

        public int dazzledTicks;
        private int illuminatedTicks;

    }

}
