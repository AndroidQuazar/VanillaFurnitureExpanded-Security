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

    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {

        static StaticConstructorClass()
        {
            ArtilleryStrikeUtility.SetCache();

            foreach (var tDef in DefDatabase<ThingDef>.AllDefs)
            {
                // Add CompPawnTracker to all Pawns
                if (typeof(Pawn).IsAssignableFrom(tDef.thingClass))
                {
                    if (tDef.comps == null)
                        tDef.comps = new List<CompProperties>();
                    tDef.comps.Add(new CompProperties(typeof(CompPawnTracker)));
                }

                // Make stone chunks catapult ammo
                else if (tDef.IsWithinCategory(ThingCategoryDefOf.StoneChunks))
                    tDef.projectileWhenLoaded = ThingDefOf.VFES_Artillery_Rock;
            }
        }

    }

}
