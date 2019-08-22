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

    public class StatPart_Retracted : StatPart
    {

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing != null && req.Thing.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
            {
                if (valueWhenRetracted != -99999)
                {
                    return $"{"VFESecurity.Retracted".Translate().CapitalizeFirst()}: {valueWhenRetracted.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Absolute)}";
                }
                else
                {
                    var explanationBuilder = new StringBuilder();
                    if (offsetWhenRetracted != 0)
                        explanationBuilder.AppendLine($"{"VFESecurity.Retracted".Translate().CapitalizeFirst()}: {offsetWhenRetracted.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Offset)}");
                    if (factorWhenRetracted != 1)
                        explanationBuilder.AppendLine($"{"VFESecurity.Retracted".Translate().CapitalizeFirst()}: {factorWhenRetracted.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Factor)}");
                    return explanationBuilder.ToString();
                }
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing != null && req.Thing.IsSubmersible(out CompSubmersible retractableComp) && retractableComp.Submerged)
            {
                if (valueWhenRetracted != -99999)
                    val = valueWhenRetracted;
                else
                {
                    val += offsetWhenRetracted;
                    val *= factorWhenRetracted;
                }
            }
        }

        private float valueWhenRetracted = -99999;
        private float offsetWhenRetracted = 0;
        private float factorWhenRetracted = 1;

    }

}
