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
using HarmonyLib;

namespace VFESecurity
{

    public class RetaliationTracker : WorldObjectComp
    {

        public override void CompTick()
        {
            if (recentRetaliationTicks > 0)
                recentRetaliationTicks--;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref recentRetaliationTicks, "recentRetaliationTicks");
            base.PostExposeData();
        }

        public int recentRetaliationTicks;


    }

}
