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

    public static class RetractableUtility
    {

        public static bool IsRetractable(this Thing thing, out CompRetractable retractableComp)
        {
            retractableComp = thing.TryGetComp<CompRetractable>();
            return retractableComp != null;
        }

    }

}
