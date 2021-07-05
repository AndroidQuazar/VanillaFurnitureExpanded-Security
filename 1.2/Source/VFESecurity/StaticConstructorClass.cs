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

    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {
        static StaticConstructorClass()
        {
            ArtilleryStrikeUtility.SetCache();
            
            var thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < thingDefs.Count; i++)
            {
                var tDef = thingDefs[i];

                // Add CompPawnTracker and CompThingTracker
                if (typeof(ThingWithComps).IsAssignableFrom(tDef.thingClass))
                {
                    if (tDef.comps == null)
                    {
                        tDef.comps = new List<CompProperties>();
                    }

                    if (typeof(Pawn).IsAssignableFrom(tDef.thingClass))
                    {
                        tDef.comps.Add(new CompProperties(typeof(CompPawnTracker)));
                    }
                    tDef.comps.Add(new CompProperties(typeof(CompThingTracker)));
                }
                else if (tDef.IsWithinCategory(ThingCategoryDefOf.StoneChunks))
                {
                    tDef.projectileWhenLoaded = ThingDefOf.VFES_Artillery_Rock;
                }
            }
        }

        
    }

}
