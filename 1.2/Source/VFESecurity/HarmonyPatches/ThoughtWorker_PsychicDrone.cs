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

    public static class Patch_ThoughtWorker_PsychicDrone
    {

        [HarmonyPatch(typeof(ThoughtWorker_PsychicDrone), "CurrentStateInternal")]
        public static class CurrentStateInternal
        {

            private const float Radius = 15;

            public static void Postfix(Pawn p, ref ThoughtState __result)
            {
                var pawnTracker = p.GetComp<CompPawnTracker>();
                if (p.Spawned && __result.StageIndex < 4)
                {
                    var psychicPylons = p.Map.listerThings.ThingsOfDef(ThingDefOf.VFES_PsychicPylon);
                    for (int i = 0; i < psychicPylons.Count; i++)
                    {
                        var pylon = psychicPylons[i];
                        var faction = pylon.Faction;
                        var powerComp = pylon.TryGetComp<CompPowerTrader>();
                        if ((powerComp == null || powerComp.PowerOn) && p.GetStatValue(RimWorld.StatDefOf.PsychicSensitivity) > 0 && (p.Faction == null || p.Faction.HostileTo(pylon.Faction)) 
                            && (p.guest == null || !p.guest.IsPrisoner || PrisonBreakUtility.IsPrisonBreaking(p)) && p.Position.InHorDistOf(pylon.Position, Radius))
                        {
                            __result = ThoughtState.ActiveAtStage(4);
                            pawnTracker.psychicPylonExposureTicks += 300; // Thoughts update every 150 ticks
                        }
                    }
                }
            }

        }
        

    }

}
