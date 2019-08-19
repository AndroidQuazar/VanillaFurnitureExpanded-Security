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

    [DefOf]
    public static class ThingDefOf
    {

        public static ThingDef VFE_ArtilleryStrikeIncoming;
        public static ThingDef VFE_ArtilleryStrikeLeaving;
        public static ThingDef VFE_ActiveArtilleryStrike;

        public static ThingDef VFES_Turret_Artillery;

    }

}
