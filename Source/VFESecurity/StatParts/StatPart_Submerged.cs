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

    public class StatPart_Submerged : StatPart_ValueOffsetFactor
    {

        protected override bool CanAffect(StatRequest req)
        {
            return req.Thing is Thing thing && thing.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged;
        }

        protected override string ExplanationText => "VFESecurity.Submerged".Translate().CapitalizeFirst();

    }

}
