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

    public static class Patch_Skyfaller
    {

        [HarmonyPatch(typeof(Skyfaller), nameof(Skyfaller.Tick))]
        public static class Patch_Tick
        {
            public static void Prefix(Skyfaller __instance) //patch the tick, not the creation - means shields can turn on in time to do something
            {
                if (__instance.Map != null && __instance.ticksToImpact <= 20)
                {
                    var thingDefExtension = __instance.def.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues;
                    ShieldGeneratorUtility.CheckIntercept(__instance, __instance.Map, thingDefExtension.shieldDamageIntercepted, DamageDefOf.Blunt, () => __instance.OccupiedRect().Cells, () => thingDefExtension.shieldDamageIntercepted > -1,
                    postIntercept: s => 
                    {
                        if (s.Energy > 0)
                        {
                            switch (__instance)
                            {
                                case DropPodIncoming dropPod:
                                    if (ShieldGeneratorUtility.CheckPodHostility(dropPod))
                                    {
                                        var innerContainer = dropPod.Contents.innerContainer;
                                        for (int i = 0; i < innerContainer.Count; i++)
                                        {
                                            var thing = innerContainer[i];
                                            if (thing is Pawn pawn)
                                                ShieldGeneratorUtility.KillPawn(pawn, dropPod.Position, dropPod.Map);
                                        }
                                        dropPod.Destroy();
                                        return;
                                    }
                                    return;
                                case DropPodLeaving _:
                                    return;
                                default:
                                    __instance.Destroy();
                                    return;
                            }
                        }
                            
                });
                }
            }
        }
        //---added---




    }

}
