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

    public class CompRearmable : ThingComp
    {

        public CompProperties_Rearmable Props => (CompProperties_Rearmable)props;
        public Graphic UnarmedGraphic
        {
            get
            {
                if (Props.unarmedGraphicData != null)
                {
                    if (cachedUnarmedGraphic == null)
                        cachedUnarmedGraphic = Props.unarmedGraphicData.GraphicColoredFor(parent);
                    return cachedUnarmedGraphic;
                }
                return null;
            }
        }

        public override string CompInspectStringExtra()
        {
            return (armed ? "VFESecurity.Armed" : "VFESecurity.NeedsRearming").Translate();
        }

        public void Rearm()
        {
            armed = true;
            Props.rearmSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            armed = Props.initiallyArmed;
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref armed, "armed");
            base.PostExposeData();
        }

        public bool armed;

        [Unsaved]
        private Graphic cachedUnarmedGraphic;

    }

}
