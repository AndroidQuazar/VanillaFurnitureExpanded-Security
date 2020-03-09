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

    public class StatPart_AmmoCrate : StatPart_ValueOffsetFactor
    {


        protected override bool CanAffect(StatRequest req)
        {
            // Check if there are any reachable ammo crates in the vicinity of the pawn if the weapon isn't single-use
            if (req.Thing is Thing thing && thing.ParentHolder is Pawn_EquipmentTracker eq && eq.pawn.Map != null && !typeof(Verb_ShootOneUse).IsAssignableFrom(thing.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.verbClass))
            {
                var ammoCrates = eq.pawn.Map.listerThings.ThingsOfDef(ThingDefOf.VFES_AmmoCrate);
                return ammoCrates.Any(c => eq.pawn.Position.InHorDistOf(c.Position, ThingDefOf.VFES_AmmoCrate.specialDisplayRadius) && !eq.pawn.Faction.HostileTo(c.Faction) &&
                c.Map.reachability.CanReach(eq.pawn.Position, c, PathEndMode.Touch, TraverseParms.For(eq.pawn)));
            }
            return false;
        }

        protected override string ExplanationText => "VFESecurity.AmmoCrate".Translate();

    }

}
