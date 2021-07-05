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

    public class HediffGiver_Dazzled : HediffGiver
    {

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            var thingTracker = pawn.GetComp<CompThingTracker>();
            if (thingTracker != null)
            {
                if (thingTracker.Dazzled)
                    TryApply(pawn);
                else if (pawn.health.hediffSet.HasHediff(hediff))
                    pawn.health.hediffSet.hediffs.RemoveAll(h => h.def == hediff);
            }
                
        }

    }

}
