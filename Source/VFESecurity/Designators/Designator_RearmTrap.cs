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

    public class Designator_RearmTrap : Designator
    {

        public Designator_RearmTrap()
        {
            defaultLabel = "VFESecurity.DesignatorRearmTraps".Translate();
            defaultDesc = "VFESecurity.DesignatorRearmTraps_Description".Translate();
            icon = TexCommand.RearmTrap;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
            soundSucceeded = SoundDefOf.Designate_Haul;
        }

        public override int DraggableDimensions => 2;

        protected override DesignationDef Designation => DesignationDefOf.VFES_RearmTrap;

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            // Out of bounds
            if (!loc.InBounds(Map))
                return false;

            // No rearmables
            if (!RearmablesInCell(loc).Any())
                return "VFESecurity.MessageMustDesignateRearmables".Translate();

            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            var thingList = c.GetThingList(Map);
            for (int i = 0; i < thingList.Count; i++)
                DesignateThing(thingList[i]);
        }

        public override void DesignateThing(Thing t)
        {
            Map.designationManager.RemoveAllDesignationsOn(t);
            Map.designationManager.AddDesignation(new Designation(t, Designation));
        }

        private IEnumerable<Thing> RearmablesInCell(IntVec3 c)
        {
            // Out of bounds
            if (!c.InBounds(Map))
                yield break;

            var thingList = c.GetThingList(Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                var thing = thingList[i];
                if (CanDesignateThing(thing).Accepted)
                    yield return thing;
            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            var rearmableComp = t.TryGetComp<CompRearmable>();
            return rearmableComp != null && !rearmableComp.armed && Map.designationManager.DesignationOn(t, Designation) == null;
        }

    }

}
