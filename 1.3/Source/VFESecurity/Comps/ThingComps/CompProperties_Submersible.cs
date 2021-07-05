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

    public class CompProperties_Submersible : CompProperties
    {

        public CompProperties_Submersible()
        {
            compClass = typeof(CompSubmersible);
        }

        public float submergedStaticSunShadowHeight;
        public int submergedPathCost;
        public float submergedDamageFactor = 1;
        public float submergedProjectileBlockChance;
        public Traversability submergedPassability;
        public GraphicData submergedGraphicData;
#pragma warning disable CS0649
        private int ticksToSubmerge;
#pragma warning restore CS0649

        public int TicksToStateChangeFor(DeployedState state)
        {
            switch (state)
            {
                case DeployedState.Submerged:
                    return ticksToSubmerge;
                default:
                    return ticksToSubmerge;
            }
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            // No CompFlickable
            if (!parentDef.HasComp(typeof(CompFlickable)))
                yield return "Does not have CompFlickable.";
        }

    }

}
