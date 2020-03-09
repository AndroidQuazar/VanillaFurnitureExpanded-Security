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

    public class CompSubmersible : ThingComp
    {

        private Graphic cachedSubmergedGraphic;
        private Effecter cachedProgressBar;
        private CompPowerTrader cachedPowerComp;

        private int ticksToStateChange;
        private DeployedState targetState;
        private DeployedState state;

        public CompProperties_Submersible Props => (CompProperties_Submersible)props;

        public float StateChangeProgress => 1 - (float)ticksToStateChange / Props.TicksToStateChangeFor(targetState);

        public int FinalisedTicksToStateChangeFor(DeployedState state)
        {
            return Mathf.RoundToInt(Mathf.Lerp(Props.TicksToStateChangeFor(state), 0, 1 - StateChangeProgress));
        }

        public bool Submerged => state == DeployedState.Submerged;

        public Graphic SubmergedGraphic
        {
            get
            {
                if (Props.submergedGraphicData != null && cachedSubmergedGraphic == null)
                    cachedSubmergedGraphic = Props.submergedGraphicData.GraphicColoredFor(parent);
                return cachedSubmergedGraphic;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            cachedPowerComp = parent.GetComp<CompPowerTrader>();
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
            bool hasPowerComp = cachedPowerComp != null;

            // Flicked or powered on; deploy building
            if ((!hasPowerComp && signal == CompFlickable.FlickedOnSignal) || signal == CompPowerTrader.PowerTurnedOnSignal)
            {
                targetState = DeployedState.Deployed;
                ticksToStateChange = FinalisedTicksToStateChangeFor(targetState);
            }

            // Flicked or powered off; submerge building
            if ((!hasPowerComp && signal == CompFlickable.FlickedOffSignal) || signal == CompPowerTrader.PowerTurnedOffSignal)
            {
                targetState = DeployedState.Submerged;
                ticksToStateChange = FinalisedTicksToStateChangeFor(targetState);
            }
        }

        public override string TransformLabel(string label)
        {
            if (Submerged)
                label += $" ({"VFESecurity.Submerged".Translate()})";
            return label;
        }

        public override string ToString()
        {
            return base.ToString() + $"state={state}, targetState={targetState}, ticksToStateChange={ticksToStateChange}";
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksToStateChange, "ticksToStateChange");
            Scribe_Values.Look(ref targetState, "targetState");
            Scribe_Values.Look(ref state, "state");
        }

    }

}
