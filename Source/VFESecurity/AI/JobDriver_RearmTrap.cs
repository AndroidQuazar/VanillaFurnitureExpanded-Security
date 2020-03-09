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

    public class JobDriver_RearmTrap : JobDriver
    {

        private const TargetIndex TrapInd = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
        }

        private CompRearmable RearmableComp => TargetThingA.TryGetComp<CompRearmable>();

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TrapInd);
            this.FailOnThingMissingDesignation(TrapInd, DesignationDefOf.VFES_RearmTrap);

            // Go to trap
            var goToToil = Toils_Goto.GotoThing(TrapInd, PathEndMode.Touch);
            goToToil.FailOnDespawnedNullOrForbidden(TrapInd);
            yield return goToToil;

            // Rearm trap
            yield return RearmToil();
        }

        private Toil RearmToil()
        {
            var rearm = new Toil();
            rearm.initAction = () =>
            {
                rearmTicksLeft = RearmableComp.Props.workToRearm;
            };
            rearm.tickAction = () =>
            {
                if (rearmTicksLeft > 0)
                    rearmTicksLeft--;

                else
                {
                    var actor = rearm.actor;
                    var trap = job.targetA.Thing;

                    // Rearm trap
                    RearmableComp.Rearm();

                    // Remove designator
                    var rearmDes = trap.Map.designationManager.DesignationOn(trap, DesignationDefOf.VFES_RearmTrap);
                    if (rearmDes != null)
                        rearmDes.Delete();

                    // Finalise
                    actor.records.Increment(RecordDefOf.VFES_TrapsRearmed);
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            rearm.FailOnCannotTouch(TrapInd, PathEndMode.Touch);
            rearm.WithProgressBar(TrapInd, () =>  1 - ((float)rearmTicksLeft / RearmableComp.Props.workToRearm));
            rearm.defaultCompleteMode = ToilCompleteMode.Never;
            return rearm;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref rearmTicksLeft, "rearmTicksLeft");
            base.ExposeData();
        }

        private int rearmTicksLeft;

    }

}
