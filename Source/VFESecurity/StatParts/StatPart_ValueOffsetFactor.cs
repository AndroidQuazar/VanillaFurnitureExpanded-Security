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

    public abstract class StatPart_ValueOffsetFactor : StatPart
    {

        public override string ExplanationPart(StatRequest req)
        {
            if (CanAffect(req))
            {
                if (value != -99999)
                {
                    return $"{ExplanationText}: {value.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Absolute)}";
                }
                else
                {
                    var explanationBuilder = new StringBuilder();
                    if (valueOffset != 0)
                        explanationBuilder.AppendLine($"{ExplanationText}: {valueOffset.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Offset)}");
                    if (valueFactor != 1)
                        explanationBuilder.AppendLine($"{ExplanationText}: {valueFactor.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Factor)}");
                    return explanationBuilder.ToString().TrimEndNewlines();
                }
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (CanAffect(req))
            {
                if (value != -99999)
                    val = value;
                else
                {
                    val += valueOffset;
                    val *= valueFactor;
                }
            }
        }

        protected abstract bool CanAffect(StatRequest req);

        protected abstract string ExplanationText
        {
            get;
        }

        private float value = -99999;
        private float valueOffset = 0;
        private float valueFactor = 1;

    }

}
