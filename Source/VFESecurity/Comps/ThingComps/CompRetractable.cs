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

    public class CompRetractable : ThingComp
    {

        private Graphic cachedRectractedGraphic;
        private Effecter cachedProgressBar;

        private int ticksToStateChange;
        private DeployedState targetState;
        private DeployedState state;

        public CompProperties_Retractable Props => (CompProperties_Retractable)props;

        public float StateChangeProgress => 1 - (float)ticksToStateChange / Props.TicksToStateChangeFor(targetState);

        public int FinalisedTicksToStateChangeFor(DeployedState state)
        {
            return Mathf.RoundToInt(Mathf.Lerp(Props.TicksToStateChangeFor(state), 0, 1 - StateChangeProgress));
        }

        public bool Retracted => state == DeployedState.Retracted;

        public Graphic RetractedGraphic
        {
            get
            {
                if (Props.retractedGraphicData != null && cachedRectractedGraphic == null)
                    cachedRectractedGraphic = Props.retractedGraphicData.GraphicColoredFor(parent);
                return cachedRectractedGraphic;
            }
        }

        public override void CompTick()
        {
            if (ticksToStateChange > 0)
            {
                ticksToStateChange--;
                DoProgressBar();
            }

            // Update state
            else
            {
                if (state != targetState)
                {
                    state = targetState;
                    parent.Map.mapDrawer.SectionAt(parent.Position).RegenerateAllLayers();
                    parent.Map.pathGrid.RecalculatePerceivedPathCostUnderThing(parent);
                }
                if (cachedProgressBar != null)
                {
                    cachedProgressBar.Cleanup();
                    cachedProgressBar = null;
                }
            }
        }

        private void DoProgressBar()
        {
            if (cachedProgressBar == null)
                cachedProgressBar = EffecterDefOf.ProgressBar.Spawn();
            else
            { 
                cachedProgressBar.EffectTick(parent, parent);
                var progressBarMote = ((SubEffecter_ProgressBar)cachedProgressBar.children[0]).mote;
                progressBarMote.progress = StateChangeProgress;
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            // Flicked on; deploy building
            if (signal == CompFlickable.FlickedOnSignal)
            {
                targetState = DeployedState.Deployed;
                ticksToStateChange = FinalisedTicksToStateChangeFor(targetState);
            }

            // Flicked off; retract building
            if (signal == CompFlickable.FlickedOffSignal)
            {
                targetState = DeployedState.Retracted;
                ticksToStateChange = FinalisedTicksToStateChangeFor(targetState);
            }
        }

        public override string TransformLabel(string label)
        {
            if (Retracted)
                label += $" ({"VFESecurity.Retracted".Translate()})";
            return label;
        }

        public override string ToString()
        {
            return base.ToString() + $"state={state}, targetState={targetState} ticksToStateChange={ticksToStateChange}";
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksToStateChange, "ticksToStateChange");
            Scribe_Values.Look(ref targetState, "targetState");
            Scribe_Values.Look(ref state, "state");
        }

    }

}
