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

    public class Building_TrapBear : Building_TrapDamager
    {

        private static readonly FloatRange DamageRandomFactorRange = new FloatRange(0.8f, 1.2f);
        private static readonly FloatRange AffectableBodySizeRange = new FloatRange(0.15f, 2.99f);
        private const float LowerHeightBodySizeThreshold = 0.35f;

        public override Graphic Graphic
        {
            get
            {
                if (rearmableComp.armed || rearmableComp.UnarmedGraphic == null)
                    return base.Graphic;
                return rearmableComp.UnarmedGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            rearmableComp = GetComp<CompRearmable>();
        }

        protected override void SpringSub(Pawn p)
        {
            if (rearmableComp.armed)
            {
                SoundDefOf.TrapSpring.PlayOneShot(new TargetInfo(Position, Map));
                rearmableComp.armed = false;
                Map.mapDrawer.SectionAt(Position).RegenerateAllLayers();
                if (!def.building.trapDestroyOnSpring && (bool)NonPublicFields.Building_Trap_autoRearm.GetValue(this))
                    Map.designationManager.AddDesignation(new Designation(this, DesignationDefOf.VFES_RearmTrap));
                if (p == null || !AffectableBodySizeRange.Includes(p.BodySize))
                    return;

                float damage = this.GetStatValue(RimWorld.StatDefOf.TrapMeleeDamage, true) * DamageRandomFactorRange.RandomInRange;
                float armourPen = damage * VerbProperties.DefaultArmorPenetrationPerDamage;
                var partHeight = p.BodySize >= LowerHeightBodySizeThreshold ? BodyPartHeight.Bottom : BodyPartHeight.Undefined;
                BodyPartRecord hitPart = p.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Stab, partHeight);

                var dinfo = new DamageInfo(DamageDefOf.Stab, damage, armourPen, instigator: this, hitPart: hitPart);
                var damResult = p.TakeDamage(dinfo);
                var logEntry = new BattleLogEntry_DamageTaken(p, RulePackDefOf.DamageEvent_TrapSpike);
                Find.BattleLog.Add(logEntry);
                damResult.AssociateWithLog(logEntry);
            }
        }

        private CompRearmable rearmableComp;

    }

}
