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
using Verse.Sound;
using RimWorld;
using HarmonyLib;

namespace VFESecurity
{

    public class Verb_Dazzle : Verb
    {

        public ExtendedVerbProperties ExtendedVerbProps => EquipmentSource.def.GetModExtension<ExtendedVerbProperties>() ?? ExtendedVerbProperties.defaultValues;

        protected override bool TryCastShot()
        {
            // Target and caster are on different maps
            if (currentTarget.HasThing && (currentTarget.Thing.Map != caster.Map) || !(currentTarget.Thing is Pawn))
                return false;

            // No LoS
            bool lineOfSight = TryFindShootLineFromTo(caster.Position, currentTarget, out ShootLine shootLine);
            if (verbProps.stopBurstWithoutLos && !lineOfSight)
                return false;

            // Throw mote at target cell
            ExtendedMoteMaker.SearchlightEffect(currentTarget.CenterVector3, caster.Map, ExtendedVerbProps.illuminatedRadius, verbProps.AdjustedFullCycleTime(this, null) - 1.TicksToSeconds());

            // Update each thing's tracker to state that they are currently illuminated and dazzled
            var curDazzledCells = GenRadial.RadialCellsAround(currentTarget.Cell, ExtendedVerbProps.illuminatedRadius, true).ToList();
            for (int i = 0; i < curDazzledCells.Count; i++)
            {
                var thingList = curDazzledCells[i].GetThingList(caster.Map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    var thing = thingList[j];
                    if (thing is ThingWithComps thingWComps)
                    {
                        var thingTracker = thingList[j].TryGetComp<CompThingTracker>();
                        if (thingTracker != null)
                        {
                            if (ExtendedVerbProps.dazzleDurationTicks > thingTracker.dazzledTicks)
                                thingTracker.dazzledTicks = ExtendedVerbProps.dazzleDurationTicks;
                            thingTracker.Illuminated = true;
                        }
                    }
                }
            }
            return true;
        }
    }

}
