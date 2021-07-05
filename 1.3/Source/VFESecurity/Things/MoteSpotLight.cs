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
using Verse.Sound;
using RimWorld;
using HarmonyLib;

namespace VFESecurity
{

    public class MoteSpotLight : Mote
    {

        protected override bool EndOfLife => AgeSecs > lifespan;

        public override float Alpha => 0.5f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref lifespan, "lifespan");
            base.ExposeData();
        }

        public float lifespan;

    }

}
