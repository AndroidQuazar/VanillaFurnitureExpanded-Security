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

    public class WorkGiver_RearmTrap : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.VFES_RearmTrap).Select(d => d.target.Thing);
        }

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // No designation
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.VFES_RearmTrap) == null)
                return false;

            // Cannot reserve
            if (!pawn.CanReserve(t, ignoreOtherReservations: forced))
                return false;

            // No movable items off trap
            foreach (var thing in t.Position.GetThingList(t.Map))
                if (thing != t && thing.def.category == ThingCategory.Item && (thing.IsForbidden(pawn) || thing.IsInValidStorage() || !HaulAIUtility.CanHaulAside(pawn, thing, out IntVec3 storeCell)))
                    return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // Move things off trap
            foreach (var thing in t.Position.GetThingList(t.Map))
            {
                if (thing != t && thing.def.category == ThingCategory.Item)
                {
                    var haulAsideJob = HaulAIUtility.HaulAsideJobFor(pawn, thing);
                    if (haulAsideJob != null)
                        return haulAsideJob;
                }
            }

            // Rearm trap
            return new Job(JobDefOf.VFES_RearmTrap, t);
        }

    }

}
