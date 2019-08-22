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
using Harmony;

namespace VFESecurity
{

    public class HediffGiver_Dazzled : HediffGiver
    {

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            var pawnTracker = pawn.GetComp<CompPawnTracker>();
            if (pawnTracker != null)
            {
                if (pawnTracker.dazzledTicks > 0)
                    TryApply(pawn);
                else if (pawn.health.hediffSet.HasHediff(hediff))
                    pawn.health.hediffSet.hediffs.RemoveAll(h => h.def == hediff);
            }
                
        }

    }

}
