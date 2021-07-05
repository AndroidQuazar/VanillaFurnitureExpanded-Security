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

    [DefOf]
    public static class ThingDefOf
    {

        public static ThingDef VFES_ArtilleryStrikeIncoming;
        public static ThingDef VFES_ArtilleryStrikeLeaving;
        public static ThingDef VFES_ActiveArtilleryStrike;

        public static ThingDef VFES_Turret_Catapult;

        public static ThingDef VFES_Turret_Artillery;
        public static ThingDef VFES_Gun_Searchlight;
        public static ThingDef VFES_PsychicPylon;
        public static ThingDef VFES_AmmoCrate;

        public static ThingDef VFES_Artillery_Rock;

        // Motes
        public static ThingDef VFES_Mote_SpotLight;

    }

}
