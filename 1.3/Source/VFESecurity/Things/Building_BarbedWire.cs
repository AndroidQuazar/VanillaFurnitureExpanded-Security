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

    public class Building_BarbedWire : Building_TrapDamager
    {

        private static readonly FloatRange DamageRandomFactorRange = new FloatRange(0.8f, 1.2f);

        protected override void SpringSub(Pawn p)
        {
            SoundDefOf.TrapSpring.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            if (p == null)
            {
                return;
            }
            float damage = this.GetStatValue(RimWorld.StatDefOf.TrapMeleeDamage, true) * DamageRandomFactorRange.RandomInRange;
            float armorPenetration = damage * VerbProperties.DefaultArmorPenetrationPerDamage;
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Stab, damage, armorPenetration, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
            DamageWorker.DamageResult damageResult = p.TakeDamage(dinfo);
            BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(p, RulePackDefOf.DamageEvent_TrapSpike, null);
            Find.BattleLog.Add(battleLogEntry_DamageTaken);
            damageResult.AssociateWithLog(battleLogEntry_DamageTaken);
        }

    }

}
