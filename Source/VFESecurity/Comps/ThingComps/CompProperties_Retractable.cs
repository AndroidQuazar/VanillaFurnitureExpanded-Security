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

    public class CompProperties_Retractable : CompProperties
    {

        public CompProperties_Retractable()
        {
            compClass = typeof(CompRetractable);
        }

        public int retractedPathCost;
        public float retractedProjectileBlockChance;
        public Traversability retractedPassability;
        public GraphicData retractedGraphicData;
        private int ticksToRetract;
        private int ticksToDeploy;

        public int TicksToStateChangeFor(DeployedState state)
        {
            switch (state)
            {
                case DeployedState.Retracted:
                    return ticksToRetract;
                default:
                    return ticksToRetract;
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
